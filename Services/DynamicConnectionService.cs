using Bridge_App.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bridge_App.Services
{
    public class DynamicConnectionService : IDynamicConnectionService
    {
        private readonly AppDbContext _mainDbContext;
        private readonly ILogger<DynamicConnectionService> _logger;
        private static readonly AsyncLocal<string?> _currentConnectionString = new();

        public DynamicConnectionService(AppDbContext mainDbContext, ILogger<DynamicConnectionService> logger)
        {
            _mainDbContext = mainDbContext;
            _logger = logger;
        }

        public async Task<string> GetDynamicConnectionAsync(int companyKey, int projectKey)
        {
            var companyProject = await _mainDbContext.CompanyProject
                .FirstOrDefaultAsync(cp => cp.companyKey == companyKey && cp.projectKey == projectKey);

            if (companyProject == null)
            {
                _logger.LogError("[DynamicConnection] No configuration found for CompanyKey={CompanyKey}, ProjectKey={ProjectKey}", companyKey, projectKey);
                throw new Exception($"No configuration found for CompanyKey={companyKey}, ProjectKey={projectKey}");
            }

            string server = companyProject.dbServer ?? throw new Exception("DbServer missing in configuration.");
            string db = companyProject.dbName ?? throw new Exception("DbName missing in configuration.");

            // Decode encoded dbUser and dbPassword using Base64
            string? user = !string.IsNullOrWhiteSpace(companyProject.dbUser)
                ? Encoding.UTF8.GetString(Convert.FromBase64String(companyProject.dbUser))
                : companyProject.dbUser;

            string? pass = !string.IsNullOrWhiteSpace(companyProject.dbPassword)
                ? Encoding.UTF8.GetString(Convert.FromBase64String(companyProject.dbPassword))
                : companyProject.dbPassword;

            // ===== SANITIZE SERVER NAME =====
            // Replace double backslashes with single backslash
            server = server.Replace(@"\\", @"\");

            var dynamicConnString = !string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(pass)
                ? $"Server={server};Database={db};User Id={user};Password={pass};Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=True;"
                : $"Server={server};Database={db};Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=True;";

            _currentConnectionString.Value = dynamicConnString;

            _logger.LogInformation(
                "[DynamicConnection] Connection string created | CompanyKey={CompanyKey} | ProjectKey={ProjectKey} | Server={Server} | Database={Database} | AuthMode={AuthMode}",
                companyKey,
                projectKey,
                server,
                db,
                (!string.IsNullOrWhiteSpace(user) ? "SQL Auth" : "Windows Auth")
            );

            return dynamicConnString;
        }


        public void SetConnectionString(string connectionString)
        {
            _currentConnectionString.Value = connectionString;
        }

        public string? GetCurrentConnectionString()
        {
            return _currentConnectionString.Value;
        }
    }
}
