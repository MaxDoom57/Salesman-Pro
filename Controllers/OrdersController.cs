using Bridge_App.Attributes;
using Bridge_App.Data;
using Bridge_App.Models;
using Bridge_App.Services;
using Bridge_App.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using static Bridge_App.DTOs.OrdersDTO;

namespace Bridge_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiKeyAuth]
    public class OrdersController : ControllerBase
    {
        private readonly ValidationService _validationService;
        private readonly IDynamicConnectionService _connectionService;
        private readonly DatabaseServices _databaseServices;

        public OrdersController(
            ValidationService validationService,
            DatabaseServices databaseServices,
            IDynamicConnectionService connectionService
        )
        {
            _validationService = validationService;
            _connectionService = connectionService;
            _databaseServices = databaseServices;
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

        [HttpGet("all")]
        public async Task<IActionResult> GetAllOrdersData()
        {
            try
            {
                using var dynamicContext = CreateDynamicContext();

                var orders = await dynamicContext.Set<OrdersModel.Orders>()
                    .Where(o => o.fApr == 1 && !o.fInAct)
                    .ToListAsync();

                var orderLines = await dynamicContext.Set<OrdersModel.OrderLines>()
                    .Where(ol => ol.fApr == true)
                    .ToListAsync();

                if (!orders.Any())
                    return Ok(new { message = "No active or approved orders found" });

                var ordersWithLines = orders.Select(o => new
                {
                    order = o,
                    orderLines = orderLines.Where(ol => ol.orderKey == o.OrdKy).ToList()
                });

                return Ok(ordersWithLines);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Failed to fetch orders data",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        [HttpGet("{orderKey}")]
        public async Task<IActionResult> GetOrderByKey(int orderKey)
        {
            try
            {
                using var dynamicContext = CreateDynamicContext();

                var order = await dynamicContext.Set<OrdersModel.Orders>()
                    .FirstOrDefaultAsync(o => o.OrdKy == orderKey && o.fApr == 1 && !o.fInAct);

                if (order == null)
                    return NotFound(new { message = $"No active or approved order found with OrdKy = {orderKey}" });

                var orderLines = await dynamicContext.Set<OrdersModel.OrderLines>()
                    .Where(ol => ol.orderKey == orderKey && ol.fApr == true)
                    .ToListAsync();

                return Ok(new
                {
                    order = order,
                    orderLines = orderLines
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Failed to fetch order data",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddOrder([FromBody] AddOrderWithLinesDTO data)
        {
            var newOrder = data.newOrder;
            var orderLines = data.orderLines;

            if (newOrder == null || !ModelState.IsValid)
                return BadRequest(new { message = "Invalid order details" });

            if (orderLines == null || !orderLines.Any())
                return BadRequest(new { message = "Order lines are required" });

            using var dynamicContext = CreateDynamicContext();
            using var transaction = await dynamicContext.Database.BeginTransactionAsync();

            try
            {
                if (!string.IsNullOrWhiteSpace(newOrder.orderTypekey.ToString()))
                {
                    bool isValidOrderTypeKey = await _validationService.IsValidOrderTypeKey("OrdTyp", newOrder.orderTypekey);
                    if (!isValidOrderTypeKey)
                        return BadRequest(new { message = $"Invalid Order Type Key: {newOrder.orderTypekey}" });
                }
                if (!string.IsNullOrWhiteSpace(newOrder.OrderType))
                {
                    bool isValidOrderType = await _validationService.IsValidOrderType(newOrder.OrderType);
                    if (!isValidOrderType)
                        return BadRequest(new { message = "Invalid Order Type" });
                }
                if (newOrder.paymentTerm.HasValue)
                {
                    bool isValidPaymentTermKey = await _validationService.IsValidPaymentTermKey((short)newOrder.paymentTerm);
                    if (!isValidPaymentTermKey)
                        return BadRequest(new { message = "Invalid payment Term" });
                }
                if (!string.IsNullOrWhiteSpace(newOrder.addressKey.ToString()))
                {
                    bool isValidAddressKey = await _validationService.IsValidAddressKey(newOrder.addressKey);
                    if (!isValidAddressKey)
                        return BadRequest(new { message = "Invalid address key" });
                }

                var orderEntity = new OrdersModel.Orders
                {
                    orderTypekey = newOrder.orderTypekey,
                    orderNumber = await _databaseServices.GetOrderNumberLast(GlobalSessionContext.Current.CompanyKey, "PURORD"),
                    fInAct = newOrder.fInAct,
                    fApr = 0,
                    companyKey = (short)GlobalSessionContext.Current.CompanyKey,
                    OrderType = newOrder.OrderType,
                    paymentTermKey = newOrder.paymentTerm,
                    orderDate = newOrder.orderDate,
                    description = newOrder.description ?? "",
                    addressKey = newOrder.addressKey,
                    accountKey = newOrder.accountKey,
                    RepKey = await _databaseServices.GetUserKey(dynamicContext, GlobalSessionContext.Current.UserId),
                    orderCategory1 = newOrder.orderCategory1,
                    discountPrecentage = newOrder.discountPrecentage.HasValue ? (float?)newOrder.discountPrecentage.Value : null,
                    salesPrice = newOrder.salesPrice,
                    enteredUserKey = await _databaseServices.GetUserKey(dynamicContext, GlobalSessionContext.Current.UserId)
                };

                dynamicContext.Set<OrdersModel.Orders>().Add(orderEntity);
                await dynamicContext.SaveChangesAsync();

                int generatedOrderKey = orderEntity.OrdKy;
                foreach (var line in orderLines)
                {
                    if (string.IsNullOrWhiteSpace(line.itemKey.ToString()))
                    {
                        return BadRequest(new { message = "Invalid ItemKey" });
                    }
                    if (newOrder.addressKey != line.addressKey)
                    {
                        return BadRequest(new { message = "Order line address key must match order address key" });
                    }

                    var lineEntity = new OrdersModel.OrderLines
                    {
                        orderKey = generatedOrderKey,
                        lineNo = line.lineNo,
                        fApr = (line.fApr.HasValue ? line.fApr.Value != 0 : true),
                        itemKey = line.itemKey,
                        itemCode = await _databaseServices.GetItemCode(dynamicContext, line.itemKey),
                        description = line.description ?? "",
                        estimateQuantity = line.estimateQuantity.HasValue ? (float?)Convert.ToSingle(line.estimateQuantity.Value) : null,
                        orderQuantity = Convert.ToDouble(line.orderQuantity),
                        costPrice = line.costPrice.HasValue ? Convert.ToDecimal(line.costPrice.Value) : 0m,
                        salesPrice = line.salesPrice.HasValue ? Convert.ToDecimal(line.salesPrice.Value) : 0m,
                        discountPrecentage = line.discountPrecentage.HasValue ? (float?)Convert.ToSingle(line.discountPrecentage.Value) : null,
                        discountAmount = line.discountAmount.HasValue ? (decimal?)Convert.ToDecimal(line.discountAmount.Value) : null,
                        addressKey = line.addressKey,
                        enteredUserKey = await _databaseServices.GetUserKey(dynamicContext, GlobalSessionContext.Current.UserId),
                        enterDateTime = line.enterDateTime
                    };

                    dynamicContext.Set<OrdersModel.OrderLines>().Add(lineEntity);
                }

                await dynamicContext.SaveChangesAsync();
                await transaction.CommitAsync();
                await _databaseServices.IncrementOrderNumberLast(GlobalSessionContext.Current.CompanyKey, "PURORD");
                return StatusCode(201, new { message = "Order added successfully" });
            }
            catch (Exception ex)
            {
                if (transaction.GetDbTransaction().Connection != null)
                    await transaction.RollbackAsync();

                return StatusCode(500, new
                {
                    message = "Failed to add order",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        [HttpDelete("{orderKey}")]
        public async Task<IActionResult> DeleteOrder(int orderKey)
        {
            using var dynamicContext = CreateDynamicContext();
            using var transaction = await dynamicContext.Database.BeginTransactionAsync();

            try
            {
                var orderSql = "UPDATE OrdMas SET fInAct = 1 WHERE OrdKy = @OrderKey";
                await dynamicContext.Database.ExecuteSqlRawAsync(orderSql, new SqlParameter("@OrderKey", orderKey));

                var orderLinesSql = "UPDATE OrdDet SET fApr = 0 WHERE OrdKy = @OrderKey";
                await dynamicContext.Database.ExecuteSqlRawAsync(orderLinesSql, new SqlParameter("@OrderKey", orderKey));

                await transaction.CommitAsync();

                return Ok(new { message = "Order soft deleted successfully" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new
                {
                    message = "Failed to soft delete order",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }
    }
}
