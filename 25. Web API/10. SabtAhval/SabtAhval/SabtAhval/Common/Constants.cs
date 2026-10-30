using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using System.Text;

namespace SabtAhval.Common
{
    public static class Constants
    {
        public const string ApiKeyHeader = "X-Api-Key";//98AHGG^%^aassjd12
        public const string ApiClientItemKey = "ApiClient";

        // مسیرهایی که نباید قفل امنیتی داشته باشند
        private static readonly string[] BypassedPrefixes = { "/swagger", "/openapi", "/health" };

        public static bool IsBypassedPath(PathString path)
            => BypassedPrefixes.Any(p => path.StartsWithSegments(p));
    
        public static string ComputeSha256Hash(string rawData)
        {
            // 1. تبدیل رشته به آرایه بایت (UTF-8)
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // 2. تبدیل آرایه بایت به رشته هگزادسیمال
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2")); // x2 = دو رقم هگزادسیمال کوچک
                }
                return builder.ToString();
            }
        }
    }
}