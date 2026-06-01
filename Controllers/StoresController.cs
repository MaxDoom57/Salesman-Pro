using Bridge_App.Attributes;
using Bridge_App.Data;
using Bridge_App.Models;
using Bridge_App.Services;
using Bridge_App.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using static Bridge_App.DTOs.StoresDTO;
using static Bridge_App.Models.StoresModel;

namespace Bridge_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiKeyAuth]
    public class StoresController : ControllerBase
    {
        private readonly ValidationService _validationService;
        private readonly DatabaseServices _databaseService;
        private readonly IDynamicConnectionService _connectionService;

        public StoresController(
            ValidationService validationService,
            DatabaseServices databaseService,
            IDynamicConnectionService connectionService
        )
        {
            _validationService = validationService;
            _databaseService = databaseService;
            _connectionService = connectionService;
        }

        // Helper to create dynamic DbContext
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

        // POST: api/Stores/add
        [HttpPost("add")]
        public async Task<IActionResult> AddLocation([FromBody] AddStoresDTO newStore)
        {
            if (newStore == null || !ModelState.IsValid)
                return BadRequest(new { message = "Invalid store details" });

            using var dynamicContext = CreateDynamicContext();
            dynamicContext.Database.SetCommandTimeout(120);
            using var transaction = await dynamicContext.Database.BeginTransactionAsync();
            try
            {
                string accountType = string.IsNullOrWhiteSpace(newStore.accountType) ? "CUS" : newStore.accountType;
                bool validAccountTypeCode = await _validationService.IsValidAccountTypeCode(accountType);
                if (!validAccountTypeCode)
                    return BadRequest(new { message = "Invalid Account Type" });

                bool isAddressCodeExist = await _validationService.isAddressCodeExist(newStore.addressCode);
                if (isAddressCodeExist)
                {
                    return BadRequest(new { message = "Store code exist" });
                }

                if (newStore.addressCode.Length >15)
                {
                    return BadRequest(new { message = "Store code lenght invalid" });
                }

                short accountTypeKey = await _databaseService.GetAccountTypeKey("CUS");
                string addressCode = newStore.addressCode;
                short addressTypeKey = await _databaseService.GetAddressTypeKey("CUS");
                int userKey = await _databaseService.GetUserKey(dynamicContext, GlobalSessionContext.Current.UserId);

                var addressSql = @"
                    INSERT INTO Address
                    (CKy, AdrCd, fInAct, AdrNm, Address, Town, City, Country, GPSLoc, ZipCd, TP1, TP2, EMail, EntUsrKy)
                    VALUES
                    (@CKy, @AdrCd, @fInAct, @AdrNm, @Address, @Town, @City, @Country, @GPSLoc, @ZipCd, @TP1, @TP2, @EMail, @EntUsrKy)";

                await dynamicContext.Database.ExecuteSqlRawAsync(
                addressSql,
                new[]
                {
                    new SqlParameter("@CKy", GlobalSessionContext.Current.CompanyKey),
                    new SqlParameter("@AdrCd", newStore.addressCode ?? (object)DBNull.Value),
                    new SqlParameter("@fInAct", System.Data.SqlDbType.Bit){ Value = 0 },
                    new SqlParameter("@AdrNm", newStore.addressName ?? (object)DBNull.Value),
                    new SqlParameter("@Address", newStore.address ?? (object)DBNull.Value),
                    new SqlParameter("@Town", newStore.town ?? (object)DBNull.Value),
                    new SqlParameter("@City", newStore.city ?? (object)DBNull.Value),
                    new SqlParameter("@Country", newStore.country ?? (object)DBNull.Value),
                    new SqlParameter("@GPSLoc", newStore.GPS ?? (object)DBNull.Value),
                    new SqlParameter("@ZipCd", newStore.zipCode ?? (object)DBNull.Value),
                    new SqlParameter("@TP1", newStore.phoneNumber1 ?? (object)DBNull.Value),
                    new SqlParameter("@TP2", newStore.phoneNumber2 ?? (object)DBNull.Value),
                    new SqlParameter("@EMail", newStore.eMail ?? (object)DBNull.Value),
                    new SqlParameter("@EntUsrKy", userKey)
                });

                var accountSql = @"
                    INSERT INTO AccMas
                    (CKy, AccCd, fInAct, fApr, AccNm, AccTyp, AccTypKy, CrLmt, BnkAccNo, BnkAccNm, EntUsrKy)
                    VALUES
                    (@CKy, @AccCd, @fInAct, @fApr, @AccNm, @AccTyp, @AccTypKy, @CrLmt, @BnkAccNo, @BnkAccNm, @EntUsrKy)";

                await dynamicContext.Database.ExecuteSqlRawAsync(accountSql,
                    new SqlParameter("@CKy", GlobalSessionContext.Current.CompanyKey),
                    new SqlParameter("@AccCd", newStore.addressCode),
                    new SqlParameter("@fInAct", System.Data.SqlDbType.Bit) { Value = 0 },
                    new SqlParameter("@fApr", System.Data.SqlDbType.Bit) { Value = 0 },
                    new SqlParameter("@AccNm", newStore.addressName ?? ""),
                    new SqlParameter("@AccTyp", "CUS"),
                    new SqlParameter("@AccTypKy", accountTypeKey),
                    new SqlParameter("@CrLmt", newStore.creditLimit ?? 0),
                    new SqlParameter("@BnkAccNo", newStore.bankAccountNumber ?? ""),
                    new SqlParameter("@BnkAccNm", newStore.bankAccountName ?? ""),
                    new SqlParameter("@EntUsrKy", userKey)
                );

                int addressKey = await _databaseService.GetAddressKeyByAdrCode(dynamicContext, addressCode);

                var addressCdRelSql = @"
                    INSERT INTO AddressCdRel
                    (AddressCdRelTypKy, AdrKy, CdKy, Lino, fApr, EntUsrKy)
                    VALUES
                    (@AddressCdRelTypKy, @AdrKy, @CdKy, @Lino, @fApr, @EntUsrKy)";

                await dynamicContext.Database.ExecuteSqlRawAsync(addressCdRelSql,
                    new SqlParameter("@AddressCdRelTypKy", System.Data.SqlDbType.SmallInt) { Value = (short)0 },
                    new SqlParameter("@AdrKy", addressKey),
                    new SqlParameter("@CdKy", addressTypeKey),
                    new SqlParameter("@Lino", System.Data.SqlDbType.SmallInt) { Value = (short)0 },
                    new SqlParameter("@fApr", true),
                    new SqlParameter("@EntUsrKy", userKey)
                );
                int accountKey = await _databaseService.GetAccountKey(dynamicContext, addressCode);
                var AccAdrSql = @"
                    INSERT INTO AccAdr
                    (AccKy, AdrKy)
                    VALUES
                    (@AccKy, @AdrKy)";

                await dynamicContext.Database.ExecuteSqlRawAsync(AccAdrSql,
                    new SqlParameter("@AccKy", accountKey),
                    new SqlParameter("@AdrKy", addressKey)
                );

                await transaction.CommitAsync();
                return StatusCode(201, new { message = "Store added successfully" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new
                {
                    message = "Failed to add store....",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        // PUT: api/Stores/update/{storeKey}
        [HttpPut("{storeKey}")]
        public async Task<IActionResult> UpdateLocation(int storeKey, [FromBody] UpdateStoresDTO updatedStore)
        {
            if (updatedStore == null || !ModelState.IsValid)
                return BadRequest(new { message = "Invalid store details" });

            using var dynamicContext = CreateDynamicContext();
            using var transaction = await dynamicContext.Database.BeginTransactionAsync();

            try
            {
                bool existCompanyKey = await _validationService.IsExistCompanyKey(Convert.ToInt32(updatedStore.companyKey));
                if (!existCompanyKey)
                    return BadRequest(new { message = "Invalid Company Key" });

                //bool isValidEnteredUser = await _validationService.IsValidUserKey(updatedStore.enteredUserKey);
                //if (!isValidEnteredUser)
                //    return BadRequest(new { message = "Invalid EnteredUser Key" });

                string accountType = string.IsNullOrWhiteSpace(updatedStore.accountType) ? "Cash" : updatedStore.accountType;
                bool validAccountTypeCode = await _validationService.IsValidAccountTypeCode(accountType);
                if (!validAccountTypeCode)
                    return BadRequest(new { message = "Invalid Account Type" });

                short accountTypeKey = await _databaseService.GetAccountTypeKey(accountType);
                string addressCode = updatedStore.addressCode;

                if (string.IsNullOrWhiteSpace(addressCode))
                    return BadRequest(new { message = "Address Code is required" });

                var updateAddressSql = @"
                    UPDATE Address
                    SET 
                        CKy = @CKy,
                        AdrNm = @AdrNm,
                        Address = @Address,
                        Town = @Town,
                        City = @City,
                        Country = @Country,
                        GPSLoc = @GPSLoc,
                        ZipCd = @ZipCd,
                        TP1 = @TP1,
                        TP2 = @TP2,
                        EMail = @EMail,
                        EntUsrKy = @EntUsrKy
                    WHERE AdrKy = @AdrKy";

                int affectedAddress = await dynamicContext.Database.ExecuteSqlRawAsync(updateAddressSql,
                    new SqlParameter("@CKy", GlobalSessionContext.Current.CompanyKey),
                    new SqlParameter("@AdrKy", storeKey),
                    new SqlParameter("@AdrNm", updatedStore.addressName ?? (object)DBNull.Value),
                    new SqlParameter("@Address", updatedStore.address ?? (object)DBNull.Value),
                    new SqlParameter("@Town", updatedStore.town ?? (object)DBNull.Value),
                    new SqlParameter("@City", updatedStore.city ?? (object)DBNull.Value),
                    new SqlParameter("@Country", updatedStore.country ?? (object)DBNull.Value),
                    new SqlParameter("@GPSLoc", updatedStore.GPS ?? (object)DBNull.Value),
                    new SqlParameter("@ZipCd", updatedStore.zipCode ?? (object)DBNull.Value),
                    new SqlParameter("@TP1", updatedStore.phoneNumber1 ?? (object)DBNull.Value),
                    new SqlParameter("@TP2", updatedStore.phoneNumber2 ?? (object)DBNull.Value),
                    new SqlParameter("@EMail", updatedStore.eMail ?? (object)DBNull.Value),
                    new SqlParameter("@EntUsrKy", await _databaseService.GetUserKey(dynamicContext, GlobalSessionContext.Current.UserId))
                );

                if (affectedAddress == 0)
                    return NotFound(new { message = "Address not found" });

                var accMapping = await dynamicContext.AccAdrModel
                    .FirstOrDefaultAsync(aa => aa.AdrKy == storeKey);

                if (accMapping == null)
                    return NotFound(new { message = "Account mapping not found" });

                int accKy = accMapping.AccKy;

                var updateAccMasSql = @"
                    UPDATE AccMas
                    SET
                        CKy = @CKy,
                        AccNm = @AccNm,
                        AccTyp = @AccTyp,
                        AccTypKy = @AccTypKy,
                        CrLmt = @CrLmt,
                        BnkAccNo = @BnkAccNo,
                        BnkAccNm = @BnkAccNm,
                        EntUsrKy = @EntUsrKy
                    WHERE AccKy = @AccKy";

                int affectedAccount = await dynamicContext.Database.ExecuteSqlRawAsync(updateAccMasSql,
                    new SqlParameter("@CKy", GlobalSessionContext.Current.CompanyKey),
                    new SqlParameter("@AccKy", accKy),
                    new SqlParameter("@AccNm", updatedStore.addressName ?? (object)DBNull.Value),
                    new SqlParameter("@AccTyp", accountType),
                    new SqlParameter("@AccTypKy", accountTypeKey),
                    new SqlParameter("@CrLmt", updatedStore.creditLimit ?? 0),
                    new SqlParameter("@BnkAccNo", updatedStore.bankAccountNumber ?? (object)DBNull.Value),
                    new SqlParameter("@BnkAccNm", updatedStore.bankAccountName ?? (object)DBNull.Value),
                    new SqlParameter("@EntUsrKy", await _databaseService.GetUserKey(dynamicContext, GlobalSessionContext.Current.UserId))
                );

                if (affectedAccount == 0)
                    return NotFound(new { message = "Account not found" });

                await transaction.CommitAsync();
                return Ok(new { message = "Store updated successfully" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new
                {
                    message = "Failed to update store",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        // DELETE: api/Stores/deleteLocation/{locationKey}
        [HttpDelete("{locationKey}")]
        public async Task<IActionResult> DeleteLocation(int locationKey)
        {
            try
            {
                using var dynamicContext = CreateDynamicContext();
                if (string.IsNullOrWhiteSpace(locationKey.ToString()))
                    return BadRequest(new { message = "Location key is required." });

                var existingLocation = await dynamicContext.Stores.FirstOrDefaultAsync(l => l.AdrKy == locationKey);
                if (existingLocation == null)
                    return NotFound(new { message = "Location not found." });

                var sql = @"UPDATE Address SET fInAct = {0} WHERE AdrKy = {1};";
                await dynamicContext.Database.ExecuteSqlRawAsync(sql, 1, locationKey);

                return StatusCode(201, new { message = "Store deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Failed to delete store.",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        // GET: api/Stores/all
        [HttpGet("all")]
        public async Task<IActionResult> GetAllLocations()
        {
            try
            {
                var dynamicContext = CreateDynamicContext();
                if (dynamicContext == null)
                    return StatusCode(500, new { message = "Database context is null" });
                int userKey = await _databaseService.GetUserKey(dynamicContext, GlobalSessionContext.Current.UserId);
                int salesRepAdrKy = await _databaseService.GetSalesRepKeyByUserKey(dynamicContext, userKey);

                var stores = await dynamicContext.ViewStores
                    .Where(s => s.fInAct == false && s.AdrTyp == "CUS" && s.SlsRepKy == salesRepAdrKy)
                    .ToListAsync();

                return Ok(stores);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Failed to fetch stores",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        // GET: api/Stores/{addressKey}
        [HttpGet("{addressKey}")]
        public async Task<IActionResult> GetLocationByKey(int addressKey)
        {
            try
            {
                using var dynamicContext = CreateDynamicContext();
                int userKey = await _databaseService.GetUserKey(dynamicContext, GlobalSessionContext.Current.UserId);
                int salesRepAdrKy = await _databaseService.GetSalesRepKeyByUserKey(dynamicContext, userKey);

                Console.WriteLine($"Fetching store with AddressKey: {addressKey} for SalesRepAdrKy: {salesRepAdrKy}");

                var store = await dynamicContext.ViewStores
                    .Where(s => s.fInAct == false && s.AdrTyp == "CUS" && s.AdrKy == addressKey && s.SlsRepKy == salesRepAdrKy)
                    .FirstOrDefaultAsync();

                if (store == null)
                    return Ok(new { message = "StoreKey not found" });

                return Ok(store);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Failed to fetch store",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

    }
}
