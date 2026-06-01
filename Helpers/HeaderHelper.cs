using Microsoft.AspNetCore.Http;
using System;
using System.Text;

namespace Bridge_App.Helpers
{
    public static class HeaderHelper
    {
        public static (string companyKey, string projectKey) ExtractCompanyProjectKeys(IHeaderDictionary headers)
        {
            const string HEADER_KEY = "x-api-key";

            if (!headers.TryGetValue(HEADER_KEY, out var secretKey))
                throw new ArgumentException("Missing header key");

            string sKey = secretKey.ToString();
            if (string.IsNullOrEmpty(sKey))
                throw new ArgumentException("Header key is empty");

            string decodedText = DecodeBase64(sKey);

            var parts = decodedText.Split(';');
            if (parts.Length < 2)
                throw new ArgumentException("Header key format invalid");

            string decodedSecondPart = DecodeBase64(parts[1]);
            string[] parts2 = decodedSecondPart.Split(';');

            if (parts2.Length < 2)
                throw new ArgumentException("Header key format invalid");

            string companyKey = parts2[0].Trim();
            string projectKey = parts2[1].Trim();

            return (companyKey, projectKey);
        }

        private static string DecodeBase64(string encodedText)
        {
            try
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(encodedText));
            }
            catch
            {
                throw new ArgumentException("Header key is not valid Base64");
            }
        }
    }
}
