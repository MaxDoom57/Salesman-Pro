using Bridge_App.Data;
using Bridge_App.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});

// Run as Windows Service
builder.Host.UseWindowsService();

// Logging (basic)
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Warning);

// FIX: Use HTTP only (no HTTPS)
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(7060);   // HTTP
});

// ============================
// JWT Authentication
// ============================
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// ============================
// DbContexts
// ============================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("StaticConnection")));

builder.Services.AddScoped<DynamicDbContextFactory>();

// ============================
// Services
// ============================
builder.Services.AddScoped<IDynamicConnectionService, DynamicConnectionService>();
builder.Services.AddScoped<ValidationService>();
builder.Services.AddScoped<DatabaseServices>();
builder.Services.AddScoped<DynamicConnectionService>();

// ============================
// Controllers
// ============================
builder.Services.AddControllers();

// ============================
// Swagger
// ============================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ============================
// Build App
// ============================
var app = builder.Build();

// ============================
// Middleware
// ============================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//  REMOVED HTTPS REDIRECTION (important for service)

app.UseMiddleware<SessionInitializerMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// ============================
// DynamicDbContextFactory
// ============================
public class DynamicDbContextFactory
{
    private readonly IDynamicConnectionService _connectionService;

    public DynamicDbContextFactory(IDynamicConnectionService connectionService)
    {
        _connectionService = connectionService;
    }

    public AppDbContext CreateDbContext()
    {
        var connectionString = _connectionService.GetCurrentConnectionString()
                              ?? throw new Exception("Dynamic connection string not set.");

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(connectionString)
                      .EnableDetailedErrors();

        return new AppDbContext(optionsBuilder.Options);
    }
}