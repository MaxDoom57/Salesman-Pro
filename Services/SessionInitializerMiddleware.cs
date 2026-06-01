using Bridge_App.Utilities;
using Bridge_App.Services;
using System.Text;
using Bridge_App.Models;
using Microsoft.AspNetCore.Http;

public class SessionInitializerMiddleware
{
    private readonly RequestDelegate _next;

    public SessionInitializerMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IDynamicConnectionService connectionService)
    {
        if (context.Request.Headers.TryGetValue("x-api-key", out var encodedKey))
        {
            try
            {
                string sKey = encodedKey.ToString().Replace(" ", "+"); // fix + mangling in transit

                // Layer 1 decode → "userId;Base64(...)"
                string decoded1 = Encoding.UTF8.GetString(Convert.FromBase64String(sKey));
                var parts1 = decoded1.Split(';');

                if (parts1.Length >= 2)
                {
                    string userId = parts1[0].Trim();
                    string part2 = parts1[1].Trim().Replace(" ", "+");

                    // Layer 2 decode → "companyKey;projectKey;salt"
                    string decoded2 = Encoding.UTF8.GetString(Convert.FromBase64String(part2));
                    var parts2 = decoded2.Split(';');

                    if (parts2.Length >= 3 &&
                        int.TryParse(parts2[0].Trim(), out int companyKey) &&
                        int.TryParse(parts2[1].Trim(), out int projectKey))
                    {
                        string connectionString = await connectionService
                            .GetDynamicConnectionAsync(companyKey, projectKey);

                        context.Items["Session"] = new SessionData
                        {
                            CompanyKey = companyKey,
                            ProjectKey = projectKey,
                            UserId = userId,
                            ConnectionString = connectionString,
                            CreatedAt = DateTime.UtcNow
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                // Silently skip — let ApiKeyAuthAttribute handle errors if invalid
            }
        }

        await _next(context); // ALWAYS continue
    }
}
