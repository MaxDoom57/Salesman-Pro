using Bridge_App.Attributes;
using Bridge_App.DTOs;
using Bridge_App.Services;
using Bridge_App.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Bridge_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiKeyAuth]
    public class AccountBalanceDetailController : ControllerBase
    {
        private readonly ValidationService _validationService;
        private readonly DynamicConnectionService _connectionService;

        public AccountBalanceDetailController(
            ValidationService validationService,
            DynamicConnectionService connectionService
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

        [HttpGet("Balance")]
        public async Task<IActionResult> GetAccountBalanceDetails(int addressKey)
        {
            try
            {
                using var dynamicContext = CreateDynamicContext();

                var parameters = new[]
                {
                    new SqlParameter("@pCKy", GlobalSessionContext.Current.CompanyKey),
                    new SqlParameter("@pDt", DateTime.Now),
                    new SqlParameter("@pAdrKy", addressKey)
                };

                var result = await dynamicContext
                    .Set<AccountBalanceDetailsDTO>()
                    .FromSqlRaw("EXEC sprAccBalDet @pCKy, @pDt, @pAdrKy", parameters)
                    .ToListAsync();

                if (result == null || result.Count == 0)
                    return Ok(new { message = "No account balance details found" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Failed to fetch account balance details",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }
    }
}
