using Bridge_App.Data;
using Bridge_App.Services;
using Bridge_App.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Cryptography;
using System.Text;
using Bridge_App.Models;


namespace Bridge_App.Attributes
{
    public class ApiKeyAuthAttribute : Attribute, IAuthorizationFilter
    {
        private const string HEADER_KEY = "x-api-key";

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var db = context.HttpContext.RequestServices.GetService<AppDbContext>();
            var dynamicConnectionService = context.HttpContext.RequestServices.GetService<IDynamicConnectionService>();

            if (!context.HttpContext.Request.Headers.TryGetValue(HEADER_KEY, out var secretKey))
            {
                context.Result = new UnauthorizedObjectResult(new { message = "API key missing" });
                return;
            }

            string sKey = secretKey.ToString().Replace(" ", "+");
            string userId, companyKey, projectKey, salt;

            if (string.IsNullOrEmpty(sKey))
            {
                context.Result = new UnauthorizedObjectResult(new { message = "API key missing" });
                return;
            }

            if (sKey.Length % 4 != 0 || !System.Text.RegularExpressions.Regex.IsMatch(sKey, @"^[A-Za-z0-9+/=]*$"))
            {
                context.Result = new UnauthorizedObjectResult(new { message = "Invalid API key" });
                return;
            }

            try
            {
                // Decode first part
                string decodedText1 = DecodeText(sKey);
                string[] textParts = decodedText1.Split(';');
                if (textParts.Length < 2)
                {
                    context.Result = new UnauthorizedObjectResult(new { message = "Invalid API key format" });
                    return;
                }

                userId = textParts[0].Trim();
                string textParts2 = textParts[1].Trim().Replace(" ", "+");

                // Decode second part
                string decodedText2 = DecodeText(textParts2);
                string[] finalText = decodedText2.Split(';');
                if (finalText.Length < 3)
                {
                    context.Result = new UnauthorizedObjectResult(new { message = "Invalid API key format" });
                    return;
                }

                companyKey = finalText[0].Trim();
                projectKey = finalText[1].Trim();
                salt = finalText[2].Trim();


                if (!IsCorrectSecretKey(context, userId, companyKey, projectKey, salt))
                {
                    context.Result = new UnauthorizedObjectResult(new { message = "Invalid API key" });
                    return;
                }

                if (dynamicConnectionService != null)
                {
                    if (!int.TryParse(companyKey, out int companyKeyInt) || !int.TryParse(projectKey, out int projectKeyInt))
                    {
                        context.Result = new UnauthorizedObjectResult(new { message = "API key contains invalid company or project key" });
                        return;
                    }

                    var dynamicConnection = dynamicConnectionService
                        .GetDynamicConnectionAsync(companyKeyInt, projectKeyInt)
                        .GetAwaiter().GetResult();

                    GlobalSessionContext.Current = new SessionData
                    {
                        CompanyKey = companyKeyInt,
                        ProjectKey = projectKeyInt,
                        UserId = userId,
                        ConnectionString = dynamicConnection,
                    };

                }
                else
                {
                    context.Result = new UnauthorizedObjectResult(new { message = "DynamicConnectionService not available" });
                    return;
                }
            }
            catch (Exception ex)
            {
                context.Result = new UnauthorizedObjectResult(new { message = ex.Message });
                return;
            }
        }

        public bool IsCorrectSecretKey(AuthorizationFilterContext context, string userId, string companyKey, string projectKey, string salt)
        {
            return IsCorrectSalt(salt)
                && IsCorrectUserId(context, userId)
                && IsCorrectCompanyKey(context, companyKey)
                && IsCorrectProjectKey(context, projectKey);
        }

        public static string EncodeText(string plainText)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(bytes);
        }

        public static string DecodeText(string encodedText)
        {
            try
            {
                byte[] bytes = Convert.FromBase64String(encodedText);
                return Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                throw new Exception("Invalid API key format");
            }
        }

        public string Hashing(string data)
        {
            using var sha256Hash = SHA256.Create();
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(data));
            return string.Concat(bytes.Select(b => b.ToString("x2")));
        }

        public bool IsCorrectSalt(string salt)
        {
            string hash = Hashing("ISSaltAddHere");
            return salt == hash;
        }

        public bool IsCorrectUserId(AuthorizationFilterContext context, string userId)
        {
            var db = context.HttpContext.RequestServices.GetService<AppDbContext>()
                     ?? throw new Exception("Database context not found");

            return db.UsrMas.Any(u => u.UsrId == userId);
        }

        public bool IsCorrectCompanyKey(AuthorizationFilterContext context, string companyKey)
        {
            var db = context.HttpContext.RequestServices.GetService<AppDbContext>()
                     ?? throw new Exception("Database context not found");

            return db.Company.Any(u => u.CKy == Convert.ToInt32(companyKey));
        }

        public bool IsCorrectProjectKey(AuthorizationFilterContext context, string projectKey)
        {
            var db = context.HttpContext.RequestServices.GetService<AppDbContext>()
                     ?? throw new Exception("Database context not found");

            return db.Company.Any(u => u.projectKey == Convert.ToInt32(projectKey));
        }
    }
}
