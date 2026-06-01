using Bridge_App.Attributes;
using Bridge_App.Services;
using Bridge_App.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using static Bridge_App.DTOs.CollectionDTO;
using static Bridge_App.DTOs.StoresDTO;

namespace Bridge_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiKeyAuth]
    public class CashCollectionController : ControllerBase
    {
        private readonly ValidationService _validationService;
        private readonly DatabaseServices _databaseService;
        private readonly IDynamicConnectionService _connectionService;

        public CashCollectionController(
            ValidationService validationService,
            DatabaseServices databaseService,
            IDynamicConnectionService connectionService
        )
        {
            _validationService = validationService;
            _databaseService = databaseService;
            _connectionService = connectionService;
        }

        private AppDbContext CreateDynamicContext()
        {
            var connectionString = GlobalSessionContext.Current.ConnectionString;
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException("Dynamic connection string is not set for this session.");

            return new AppDbContext(connectionString);
        }

        //[HttpGet("session")]
        //public IActionResult GetSession()
        //{
        //    var session = GlobalSessionContext.Current;
        //    if (session == null)
        //        return Ok("No session");
        //    return Ok(session);
        //}

        //-------------------------------------------------
        //Endpoint start from here
        //-------------------------------------------------

        [HttpPost("add")]
        public async Task<IActionResult> AddCashCollection([FromBody] AddCollectionDTO newCashCollection)
        {
            if (newCashCollection == null || !ModelState.IsValid)
                return BadRequest(new { message = "Invalid Cash collection details" });

            using var dynamicContext = CreateDynamicContext();
            using var transaction = await dynamicContext.Database.BeginTransactionAsync();

            try
            {
                DateTime trnDate = DateTime.Now;

                bool isValidPaymentMode = await _validationService.IsValidPaymentMode(newCashCollection.paymentMode);
                if (!isValidPaymentMode)
                    return BadRequest(new { message = "Invalid payment mode" });

                bool isValidAddressKey = await _validationService.IsValidAddressKey(newCashCollection.addressKey);
                if (!isValidAddressKey)
                    return BadRequest(new { message = "Invalid address key" });

                var collectionSql = @"
            INSERT INTO CollectionDt
            (CKy, ColNo, ColId, Amt, TrnDt, AdrKy, PmtMode, ChqNo, RepKy, fInAct, fSync, fPost, PostUsrKy)
            VALUES
            (@CKy, @ColNo, @ColId, @Amt, @TrnDt, @AdrKy, @PmtMode, @ChqNo, @RepKy, 0, 0, 0, 0)";

                await dynamicContext.Database.ExecuteSqlRawAsync(
                    collectionSql,
                    new SqlParameter("@CKy", GlobalSessionContext.Current.CompanyKey),
                    new SqlParameter("@ColNo", newCashCollection.collectionNumber),
                    new SqlParameter("@ColId", newCashCollection.collectionId),
                    new SqlParameter("@Amt", newCashCollection.amount),
                    new SqlParameter("@TrnDt", trnDate),
                    new SqlParameter("@AdrKy", newCashCollection.addressKey),
                    new SqlParameter("@PmtMode", newCashCollection.paymentMode),
                    new SqlParameter("@ChqNo", (object?)newCashCollection.chequeNo ?? DBNull.Value),
                    new SqlParameter("@RepKy", await _databaseService.GetUserKey(dynamicContext, GlobalSessionContext.Current.UserId))
                );

                await transaction.CommitAsync();
                return StatusCode(201, new { message = "Collection added successfully" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new
                {
                    message = "Failed to add cash collection",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }
    }
}
