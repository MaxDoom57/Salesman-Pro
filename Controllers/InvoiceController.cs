using Bridge_App.Attributes;
using Bridge_App.DTOs;
using Bridge_App.Services;
using Bridge_App.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
using System.Data.Common;
using System.Reflection.Emit;
using static Bridge_App.DTOs.InvoiceDTO;
using static Bridge_App.Models.InvoiceModel;

namespace Bridge_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiKeyAuth]
    public class InvoiceController : ControllerBase
    {
        private readonly ValidationService _validationService;
        private readonly DatabaseServices _databaseService;
        private readonly IDynamicConnectionService _connectionService;

        public InvoiceController(
            ValidationService validationService,
            DatabaseServices databaseService,
            IDynamicConnectionService connectionService
        )
        {
            _validationService = validationService;
            _databaseService = databaseService;
            _connectionService = connectionService;
        }

        // Helper: Create dynamic DbContext
        private AppDbContext CreateDynamicContext()
        {
            var connectionString = GlobalSessionContext.Current.ConnectionString;
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException("Dynamic connection string is not set for this session.");

            return new AppDbContext(connectionString);
        }


        // -----------------------
        // ADD Invoice
        // -----------------------
        [HttpPost("add")]
        public async Task<IActionResult> AddInvoice([FromBody] AddInvoiceDTO newInvoice)
        {
            if (newInvoice == null || !ModelState.IsValid)
                return BadRequest(new { message = "Invalid invoice details" });

            bool isValidAddressKey = await _validationService.IsValidAddressKey(newInvoice.AddressKey);
            if (!isValidAddressKey)
                return BadRequest(new { message = "Invalid address key." });

            await using var dynamicContext = CreateDynamicContext();
            await using var connection = dynamicContext.Database.GetDbConnection();

            try
            {
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                // --- Initialize helper values ---
                string ourCode = "SALE";
                short tranTypeKey = await _databaseService.GetTranTypeKey("Sales");
                int tranNoLast = await _databaseService.GetTranNumberLast(GlobalSessionContext.Current.CompanyKey, ourCode);

                int userKey = await _databaseService.GetUserKey(dynamicContext, GlobalSessionContext.Current.UserId);
                int addressKey = newInvoice.AddressKey;
                int accountKey = await _databaseService.GetAccountKeyByAdrKy(addressKey);

                short payementTermKey1 = (short)((await _databaseService.GetPaymentTermKey(newInvoice.PaymentTerm1)) ?? throw new Exception("Invalid Payment Term"));
                short payementTermKey2 = newInvoice.PaymentTerm2 != null
                    ? (short)((await _databaseService.GetPaymentTermKey(newInvoice.PaymentTerm2)) ?? 0)
                    : (short)0;

                await using var transaction = await connection.BeginTransactionAsync();

                int tranKey;

                // --- 1️⃣ Insert into TrnMas ---
                const string insertTrnMasSql = @"
                    INSERT INTO TrnMas
                    (CKy, TrnDt, TrnTypKy, TrnNo, fInAct, fApr, RtnAmt, RepAdrKy, OurCd, DocNo, YurRef,
                     Adrky, AccKy, PmtTrmKy, PmtTrmKy2, PmtTrm1Amt, PmtTrm2Amt, Amt, BUKy, LocKy, DisPer, Des, EntUsrKy, EntDtm)
                    OUTPUT INSERTED.TrnKy
                    VALUES
                    (@CKy, @TrnDt, @TrnTypKy, @TrnNo, @fInAct, @fApr, @RtnAmt, @RepAdrKy, @OurCd, @DocNo, @YurRef,
                     @Adrky, @AccKy, @PmtTrmKy, @PmtTrmKy2, @PmtTrm1Amt, @PmtTrm2Amt, @Amt, @BUKy, @LocKy, @DisPer, @Des, @EntUsrKy, @EntDtm);";

                await using (var command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandText = insertTrnMasSql;

                    command.Parameters.AddRange(new[]
                    {
                        new SqlParameter("@CKy", GlobalSessionContext.Current.CompanyKey),
                        new SqlParameter("@TrnDt", DateTime.Now),
                        new SqlParameter("@TrnTypKy", tranTypeKey),
                        new SqlParameter("@TrnNo", tranNoLast),
                        new SqlParameter("@fInAct", false),
                        new SqlParameter("@fApr", true),
                        //new SqlParameter("@STrnKy", newInvoice.STrnKy), 
                        new SqlParameter("@RtnAmt", newInvoice.ReturnAmount ?? 0m),
                        new SqlParameter("@RepAdrKy", newInvoice.RepAddressKey),
                        new SqlParameter("@OurCd", ourCode),
                        new SqlParameter("@DocNo", (object?)newInvoice.DocNo ?? DBNull.Value),
                        new SqlParameter("@YurRef", (object?)newInvoice.YourReference ?? DBNull.Value),
                        new SqlParameter("@Adrky", addressKey),
                        new SqlParameter("@AccKy", accountKey),
                        new SqlParameter("@PmtTrmKy", payementTermKey1),
                        new SqlParameter("@PmtTrmKy2", payementTermKey2),
                        new SqlParameter("@PmtTrm1Amt", newInvoice.PmtTrm1Amount),
                        new SqlParameter("@PmtTrm2Amt", newInvoice.PmtTrm2Amount ?? 0m),
                        new SqlParameter("@Amt", newInvoice.Amount),
                        new SqlParameter("@BUKy", newInvoice.BUKy ?? (short)0),
                        new SqlParameter("@LocKy", newInvoice.LocationKey),
                        new SqlParameter("@DisPer", newInvoice.DiscountPresentage ?? 0m),
                        new SqlParameter("@Des", (object?)newInvoice.Description ?? " "),
                        new SqlParameter("@EntUsrKy", userKey),
                        new SqlParameter("@EntDtm", DateTime.Now)
                    });

                    var result = await command.ExecuteScalarAsync();
                    tranKey = Convert.ToInt32(result);
                }

                // --- 2️⃣ Insert into ItmTrn ---
                const string itmTrnSql = @"
                    INSERT INTO ItmTrn
                    (TrnKy, LiNo, CKy, RefItmTrnKy, ItmKy, ItmTrnRem, LocKy, Qty, DisAmt, DisPer, CosPri, SlsPri, TrnPri, EntUsrKy, DlvrQty)
                    VALUES
                    (@TrnKy, @LiNo, @CKy, @RefItmTrnKy, @ItmKy, @ItmTrnRem, @LocKy, @Qty, @DisAmt, @DisPer, @CosPri, @SlsPri, @TrnPri, @EntUsrKy, @DeliveryQty)";

                int lineNo = 1;
                foreach (var item in newInvoice.ItemTransactions)
                {
                    await using var cmd = connection.CreateCommand();
                    cmd.Transaction = transaction;
                    cmd.CommandText = itmTrnSql;
                    cmd.Parameters.AddRange(new[]
                    {
                        new SqlParameter("@TrnKy", tranKey),
                        new SqlParameter("@LiNo", lineNo++),
                        new SqlParameter("@CKy", GlobalSessionContext.Current.CompanyKey),
                        new SqlParameter("@RefItmTrnKy", DBNull.Value),
                        new SqlParameter("@ItmKy", item.ItemKey),
                        new SqlParameter("@ItmTrnRem", " "),
                        new SqlParameter("@LocKy", item.LocationKey),
                        new SqlParameter("@Qty", item.Quantity),
                        new SqlParameter("@DisAmt", item.DiscountAmount),
                        new SqlParameter("@DisPer", item.DiscountPresentage),
                        new SqlParameter("@CosPri", item.CostPrice),
                        new SqlParameter("@SlsPri", item.SalesPrice),
                        new SqlParameter("@TrnPri", item.TranPrice),
                        new SqlParameter("@EntUsrKy", userKey),
                        new SqlParameter("@DeliveryQty", item.DeliveryQuantity)
                    });
                    await cmd.ExecuteNonQueryAsync();
                }

                // --- 3️⃣ Insert into AccTrn ---
                double cashAccKy = 0;
                double cardAccKy = 0;
                double voucherAccKy = 0;
                int accountKey1 = 0;
                int accountKey2 = 0;

                var cds = await _databaseService.GetSaleTransactionCodes(GlobalSessionContext.Current.CompanyKey);
                if (cds == null)
                {
                     cashAccKy = 0;
                     cardAccKy = 0;
                     voucherAccKy = 0;
                }
                else
                {
                     cashAccKy = cds.CdNo1;
                     cardAccKy = cds.CdNo2;
                     voucherAccKy = cds.CdNo3;
                }

                if(newInvoice.PaymentTerm1.ToUpper() == "CASH")
                {
                    accountKey1 = (int)cashAccKy;
                }
                else if(newInvoice.PaymentTerm1.ToUpper() == "CRCRD")
                {
                    accountKey1 = (int)cardAccKy;
                }
                else if(newInvoice.PaymentTerm1.ToUpper() == "CHEQUE")
                {
                    accountKey1 = (int)voucherAccKy;
                }

                if(newInvoice.PaymentTerm2 != null)
                {
                    if (newInvoice.PaymentTerm2.ToUpper() == "CASH")
                    {
                        accountKey2 = (int)cashAccKy;
                    }
                    else if (newInvoice.PaymentTerm2.ToUpper() == "CRCRD")
                    {
                        accountKey2 = (int)cardAccKy;
                    }
                    else if (newInvoice.PaymentTerm2.ToUpper() == "CHEQUE")
                    {
                        accountKey2 = (int)voucherAccKy;
                    }
                }
                else
                {
                    accountKey2 = 0;
                }

                int salesAccountKey = await _databaseService.GetDefaultSalesAccountKey((short)GlobalSessionContext.Current.CompanyKey);

                decimal totalInvoiceAmount = newInvoice.totalAmount - newInvoice.discountAmount;
                decimal pmtTrm2Amt = newInvoice.PmtTrm2Amount ?? 0;
                short paymentModeKey = await _databaseService.GetPaymentModeKey(payementTermKey1);


                async Task InsertAccTrn(int tranKey, int liNo, int accKy, decimal amt)
                {
                    await using var cmd = connection.CreateCommand();
                    cmd.Transaction = transaction;
                    cmd.CommandText = @"
                        INSERT INTO AccTrn (TrnKy, LiNo, AccKy, PmtModeKy, Amt, EntUsrKy)
                        VALUES (@TrnKy, @LiNo, @AccKy, @PmtModeKy, @Amt, @EntUsrKy)";
                    cmd.Parameters.AddRange(new[]
                    {
                        new SqlParameter("@TrnKy", tranKey),
                        new SqlParameter("@LiNo", liNo),
                        new SqlParameter("@AccKy", accKy),
                        new SqlParameter("@PmtModeKy", paymentModeKey),
                        new SqlParameter("@Amt", amt),
                        new SqlParameter("@EntUsrKy", userKey)
                    });
                    await cmd.ExecuteNonQueryAsync();
                }

                await InsertAccTrn(tranKey, 1, accountKey1, totalInvoiceAmount - pmtTrm2Amt);
                await InsertAccTrn(tranKey, 2, salesAccountKey, -(totalInvoiceAmount));
                await InsertAccTrn(tranKey, 3, accountKey2, pmtTrm2Amt);

                await _databaseService.IncrementTranNumberLast(GlobalSessionContext.Current.CompanyKey, ourCode);
                await transaction.CommitAsync();

                return StatusCode(201, new { message = "Invoice added successfully"});
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Failed to add invoice....",
                    error = ex.Message,
                    exceptionType = ex.GetType().FullName,
                    stackTrace = ex.StackTrace,
                    innerError = ex.InnerException?.Message
                });
            }
        }



        // GET invoice number
        [HttpGet("invoiceNo")]
        public async Task<IActionResult> GetAllLocations()
        {
            try
            {
                using var dynamicContext = CreateDynamicContext();
                string ourCode = "SALE";
                int tranNoLast = await _databaseService.GetTranNumberLast(GlobalSessionContext.Current.CompanyKey, ourCode);


                if (tranNoLast == 0)
                {
                    return NotFound(new { message = "No invoice number found." });
                }
                return Ok(new { invoiceNo = tranNoLast });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Failed to fetch invoice number!",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }
    }
}
