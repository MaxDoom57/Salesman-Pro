using Bridge_App.Attributes;
using Bridge_App.Data;
using Bridge_App.Helpers;
using Bridge_App.Models;
using Bridge_App.Services;
using Bridge_App.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using static Bridge_App.DTOs.ItemsDTO;

namespace Bridge_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiKeyAuth]
    public class ItemsController : ControllerBase
    {
        private readonly ValidationService _validationService;
        private readonly IDynamicConnectionService _connectionService;

        public ItemsController(
            ValidationService validationService,
            IDynamicConnectionService connectionService
        )
        {
            _validationService = validationService;
            _connectionService = connectionService;
        }

        // Helper to get dynamic DbContext using the current session's connection string
        private AppDbContext CreateDynamicContext()
        {
            var connectionString = GlobalSessionContext.Current?.ConnectionString
                                   ?? throw new Exception("Dynamic connection string is not set for this session.");

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new AppDbContext(optionsBuilder.Options);
        }

        //[HttpGet("session")]
        //public IActionResult GetSession()
        //{
        //    var session = GlobalSessionContext.Current;
        //    if (session == null)
        //        return Ok("No session");
        //    return Ok(session);
        //}


        [HttpGet("all")]
        public async Task<IActionResult> GetAllItems()
        {
            try
            {
                using var dynamicContext = CreateDynamicContext();
                var items = await dynamicContext.vewItmMas.ToListAsync();

                if (items == null || items.Count == 0)
                    return Ok(new { message = "No items found" });

                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Failed to fetch items",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        [HttpGet("{itemKey}")]
        public async Task<IActionResult> GetItemByKey(int itemKey)
        {
            try
            {
                using var dynamicContext = CreateDynamicContext();
                var item = await dynamicContext.vewItmMas.FindAsync(itemKey);

                if (item == null)
                    return Ok(new { message = "ItemKey not found" });

                return Ok(item);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Failed to fetch item",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddItem([FromBody] AddItemDTO newItem)
        {
            if (newItem == null || !ModelState.IsValid)
                return BadRequest(new { message = "Invalid item details" });

            using var dynamicContext = CreateDynamicContext();

            try
            {
                // Validation
                if (!await _validationService.IsExistCompanyKey(newItem.companyKey))
                    return BadRequest(new { message = "Invalid Company Key" });

                if (await _validationService.IsExistItemCode(newItem.itemCode))
                    return Conflict(new { message = "Item Code already exists" });

                if (!await _validationService.IsExistItemType(newItem.itemType, newItem.itemTypeKey))
                    return BadRequest(new { message = "Invalid ItemTypeKey" });

                if (!await _validationService.IsValidUnitKey(newItem.unitKey))
                    return BadRequest(new { message = "Invalid UnitKey" });

                if (!await _validationService.IsValidUserKey(newItem.enteredUser))
                    return BadRequest(new { message = "Invalid EnteredUser Key" });

                // Insert into DB
                var sql = @"INSERT INTO ItmMas(CKy, ItmCd, ItmTypKy, ItmTyp, PartNo, ItmNm, Des, LocKy, ItmCat1Ky, ItmCat2Ky, ItmCat3Ky, ItmCat4Ky, UnitKy, DisPer, CosPri, SlsPri, DisAmt, Qty, EntUsrKy, fInAct)
                            VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, {18}, 0);";

                object[] parameters = {
                    newItem.companyKey, newItem.itemCode, newItem.itemTypeKey, newItem.itemType,
                    newItem.partNo, newItem.itemName, newItem.description, newItem.locationKey,
                    newItem.itmCat1Ky, newItem.itmCat2Ky, newItem.itmCat3Ky, newItem.itmCat4Ky,
                    newItem.unitKey, newItem.discountPrecentage, newItem.costPrice, newItem.salesPrice,
                    newItem.discountAmount, newItem.quantity, newItem.enteredUser
                };

                await dynamicContext.Database.ExecuteSqlRawAsync(sql, parameters);

                return StatusCode(201, new { message = "Item added successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Failed to add item",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        [HttpPut("{itemKey}")]
        public async Task<IActionResult> UpdateItem(int itemKey, [FromBody] UpdateItemDTO updatedItem)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid item details" });

            using var dynamicContext = CreateDynamicContext();

            try
            {
                var exists = await dynamicContext.Items.AnyAsync(i => i.ItmKy == itemKey);
                if (!exists)
                    return NotFound(new { message = "Item not found" });

                // Validation
                if (!await _validationService.IsExistCompanyKey(updatedItem.companyKey))
                    return BadRequest(new { message = "Invalid Company Key" });

                if (!await _validationService.IsExistItemType(updatedItem.itemType, updatedItem.itemTypeKey))
                    return BadRequest(new { message = "Invalid ItemTypeKey" });

                if (!await _validationService.IsValidUnitKey(updatedItem.unitKey))
                    return BadRequest(new { message = "Invalid UnitKey" });

                if (!await _validationService.IsValidUserKey(updatedItem.enteredUser))
                    return BadRequest(new { message = "Invalid EnteredUser Key" });

                                            var sql = @"
                            UPDATE ItmMas
                            SET
                                CKy        = @CKy,
                                ItmCd      = @ItmCd,
                                ItmTypKy   = @ItmTypKy,
                                ItmTyp     = @ItmTyp,
                                PartNo     = @PartNo,
                                ItmNm      = @ItmNm,
                                Des        = @Des,
                                LocKy      = @LocKy,
                                ItmCat1Ky  = @ItmCat1Ky,
                                ItmCat2Ky  = @ItmCat2Ky,
                                ItmCat3Ky  = @ItmCat3Ky,
                                ItmCat4Ky  = @ItmCat4Ky,
                                UnitKy     = @UnitKy,
                                DisAmt     = @DisAmt,
                                CosPri     = @CosPri,
                                SlsPri     = @SlsPri,
                                Qty        = @Qty,
                                DisPer     = @DisPer,
                                EntUsrKy  = @EntUsrKy
                            WHERE ItmKy = @ItmKy;
                            ";

                await dynamicContext.Database.ExecuteSqlRawAsync(
                    sql,
                    new SqlParameter("@CKy", updatedItem.companyKey),
                    new SqlParameter("@ItmCd", updatedItem.itemCode),
                    new SqlParameter("@ItmTypKy", updatedItem.itemTypeKey),
                    new SqlParameter("@ItmTyp", updatedItem.itemType),
                    new SqlParameter("@PartNo", (object?)updatedItem.partNo ?? DBNull.Value),
                    new SqlParameter("@ItmNm", updatedItem.itemName),
                    new SqlParameter("@Des", (object?)updatedItem.description ?? DBNull.Value),
                    new SqlParameter("@LocKy", updatedItem.locationKey),
                    new SqlParameter("@ItmCat1Ky", (object?)updatedItem.itmCat1Ky ?? DBNull.Value),
                    new SqlParameter("@ItmCat2Ky", (object?)updatedItem.itmCat2Ky ?? DBNull.Value),
                    new SqlParameter("@ItmCat3Ky", (object?)updatedItem.itmCat3Ky ?? DBNull.Value),
                    new SqlParameter("@ItmCat4Ky", (object?)updatedItem.itmCat4Ky ?? DBNull.Value),
                    new SqlParameter("@UnitKy", updatedItem.unitKey),
                    new SqlParameter("@DisAmt", (object?)updatedItem.discountAmount ?? DBNull.Value),
                    new SqlParameter("@CosPri", updatedItem.costPrice),
                    new SqlParameter("@SlsPri", updatedItem.salesPrice),
                    new SqlParameter("@Qty", (object?)updatedItem.quantity ?? DBNull.Value),
                    new SqlParameter("@DisPer", (object?)updatedItem.discountPrecentage ?? DBNull.Value),
                    new SqlParameter("@EntUsrKy", updatedItem.enteredUser),
                    new SqlParameter("@ItmKy", itemKey)
                );
                return Ok(new { message = "Item updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Failed to update item",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }


        [HttpDelete("{itemKey}")]
        public async Task<IActionResult> DeleteItem(int itemKey)
        {
            using var dynamicContext = CreateDynamicContext();

            try
            {
                var existingItem = await dynamicContext.Items.FirstOrDefaultAsync(i => i.ItmKy == itemKey);
                if (existingItem == null)
                    return NotFound(new { message = "Item not found" });

                var sql = @"UPDATE ItmMas SET fInAct = {0} WHERE ItmKy = {1};";
                await dynamicContext.Database.ExecuteSqlRawAsync(sql, 1, itemKey);

                return Ok(new { message = "Item deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Failed to delete item.",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }
    }
}
