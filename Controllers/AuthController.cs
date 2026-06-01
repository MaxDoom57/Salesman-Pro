using Bridge_App.Data;
using Bridge_App.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Bridge_App.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private const string HEADER_KEY = "x-api-key";

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("auth")]
        public IActionResult VerifySecretKey()
        {
            // 1. Get secret key from header
            if (!HttpContext.Request.Headers.TryGetValue(HEADER_KEY, out var secretKey))
            {
                return Unauthorized(new { message = "API key missing" });
            }

            string sKey = secretKey.ToString();

            // 2. Basic validation
            if (string.IsNullOrEmpty(sKey))
                return Unauthorized(new { message = "API key missing" });

            if (!System.Text.RegularExpressions.Regex.IsMatch(sKey, @"^[A-Za-z0-9+/=]*$"))
                return Unauthorized(new { message = "Invalid API key characters" });

            try
            {
                // 3. Decode Base64 safely
                string decodedText1 = SafeBase64Decode(sKey);
                string[] textParts = decodedText1.Split(';');

                if (textParts.Length < 2)
                    return Unauthorized(new { message = "Malformed API key 1" });

                string userId = textParts[0].Trim();
                string nestedPart = textParts[1].Trim();

                string decodedText2 = SafeBase64Decode(nestedPart);
                string[] finalParts = decodedText2.Split(';');

                if (finalParts.Length < 3)
                    return Unauthorized(new { message = "Malformed API key 2" });

                string companyKey = finalParts[0].Trim();
                string projectKey = finalParts[1].Trim();
                string salt = finalParts[2].Trim();

                // 4. Validate secret key
                if (!IsCorrectSecretKey(userId, companyKey, projectKey, salt))
                    return Unauthorized(new { message = "Invalid API key" });

                return Ok(new { message = "Authorized"});
            }
            catch
            {
                return Unauthorized(new { message = "Invalid Base64 or malformed API key" });
            }
        }

        // ======= Helper Methods =======
        private bool IsCorrectSecretKey(string userId, string companyKey, string projectKey, string salt)
        {
            return IsCorrectSalt(salt)
                && IsCorrectUserId(userId)
                && IsCorrectCompanyKey(companyKey)
                && IsCorrectProjectKey(projectKey);
        }

        private static string SafeBase64Decode(string input)
        {
            input = input.Replace('-', '+').Replace('_', '/');
            switch (input.Length % 4)
            {
                case 2: input += "=="; break;
                case 3: input += "="; break;
            }

            byte[] bytes = Convert.FromBase64String(input);
            return Encoding.UTF8.GetString(bytes);
        }

        private string Hashing(string data)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        private bool IsCorrectSalt(string salt)
        {
            string expectedHash = Hashing("ISSaltAddHere");
            return salt == expectedHash;
        }

        private bool IsCorrectUserId(string userId)
        {
            return _context.UsrMas.Any(u => u.UsrId == userId);
        }

        private bool IsCorrectCompanyKey(string companyKey)
        {
            return _context.Company.Any(c => c.CKy == Convert.ToInt32(companyKey));
        }

        private bool IsCorrectProjectKey(string projectKey)
        {
            return _context.Company.Any(c => c.projectKey == Convert.ToInt32(projectKey));
        }
    }
}
