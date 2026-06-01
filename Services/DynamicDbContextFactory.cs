using Bridge_App.Data;
using Bridge_App.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Bridge_App.Services
{
    public class DynamicDbContextFactory
    {
        private readonly DbContextOptions<AppDbContext> _baseOptions;

        public DynamicDbContextFactory(DbContextOptions<AppDbContext> baseOptions)
        {
            _baseOptions = baseOptions;
        }

        public AppDbContext CreateDbContext()
        {
            var session = GlobalSessionContext.Current;
            if (session == null || string.IsNullOrEmpty(session.ConnectionString))
                throw new Exception("No active dynamic session found.");

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(session.ConnectionString)
                          .EnableDetailedErrors();

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
