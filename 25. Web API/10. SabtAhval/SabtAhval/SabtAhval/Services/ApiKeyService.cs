using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;
using SabtAhval.Data;
using SabtAhval.Models.Entities;
using SabtAhval.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace SabtAhval.Services
{
    public class ApiKeyService:IApiKeyService
    {
        private readonly AppDbContext _context;
        public ApiKeyService(AppDbContext context)
        {
            _context = context;
        }
        public async  Task<ApiClient?> GetByApiKeyAsync(string key)
        {

            if (string.IsNullOrWhiteSpace(key))
                return null;
            string keyHash = ComputeSha256Hash(key);
            var client = await _context.apiClients
                        .Include(c => c.TargetApiServerId)   // اگر به اطلاعات سرور مقصد نیاز دارید
                        .AsNoTracking()                     // چون فقط می‌خوانیم، بهینه‌تر است
                        .FirstOrDefaultAsync(c => c.ApiKeyHash == keyHash && c.IsActive);

            return client;
        }

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

