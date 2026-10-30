using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;
using SabtAhval.Common;
using SabtAhval.Data;
using SabtAhval.Models.Entities;
using SabtAhval.Services.Interfaces;
using System.Reflection.Metadata;
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
            string keyHash = Constants.ComputeSha256Hash(key);
            var client = await _context.apiClients
                        .Include(c => c.TargetApiServer)   // اگر به اطلاعات سرور مقصد نیاز دارید
                        .AsNoTracking()                     // چون فقط می‌خوانیم، بهینه‌تر است
                        .FirstOrDefaultAsync(c => c.ApiKeyHash == keyHash && c.IsActive);

            return client;
        }

    }
}

