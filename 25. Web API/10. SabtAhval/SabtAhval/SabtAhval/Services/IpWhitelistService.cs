using Microsoft.EntityFrameworkCore;
using SabtAhval.Data;

namespace SabtAhval.Services
{
    public class IpWhitelistService
    {
        private readonly AppDbContext _context;

        public IpWhitelistService(AppDbContext context)
        {
            _context = context;
        }


        public async Task<bool> IsIpAllowedAsync(string ip, int? clientId)
        {
            // 1. اعتبارسنجی ورودی
            if (string.IsNullOrWhiteSpace(ip))
                return false;

            // 2. اگر clientId مشخص نشده باشد، IP باید در لیست عمومی مجاز باشد
            //    (یعنی رکوردی با ApiClientId = null وجود ندارد در مدل شما،
            //     پس در این حالت فقط IP را بدون فیلتر کلاینت بررسی می‌کنیم)
            var query = _context.allowedIps
                .AsNoTracking()
                .Where(x => x.IsActive && x.IpAddress == ip);

            // 3. اگر clientId مقدار داشته باشد، فقط IPهای همان کلاینت را بررسی کن
            if (clientId.HasValue)
            {
                query = query.Where(x => x.ApiClientId == clientId.Value);
            }

            // 4. آیا حداقل یک رکورد مجاز فعال وجود دارد؟
            return await query.AnyAsync();
        }
    }
}
