using JwtDemoMVC.Data;
using JwtDemoMVC.Models;
using Microsoft.EntityFrameworkCore;

namespace JwtDemoMVC.Repositories;

/// <summary>
/// قرارداد Repository برای IPهای مجاز — ویژگی IP AllowList.
/// </summary>
public interface IAllowedIpRepository : IRepository<AllowedIp>
{
    // پیدا کردن یک رکورد با IP نرمال‌شده — برای جلوگیری از ثبت تکراری
    Task<AllowedIp?> GetByIpAsync(string ipAddress);

    // فقط IPهای فعال (اعتبارشان تمام نشده) — Middleware با همین لیست مقایسه می‌کند
    Task<IReadOnlyList<AllowedIp>> GetActiveIpsAsync();

    // همه‌ی رکوردها برای جدول صفحه تنظیمات — نزدیک‌ترین انقضا بالاتر
    Task<IReadOnlyList<AllowedIp>> GetAllSortedAsync();
}

public class AllowedIpRepository : Repository<AllowedIp>, IAllowedIpRepository
{
    public AllowedIpRepository(AppDbContext context) : base(context)
    {
    }

    // ── بهینه‌سازی: این سه متد فقط «خواندن» هستند؛ AsNoTracking از هزینه‌ی
    //    Change Tracking (کپی/مقایسه‌ی وضعیت Entityها) در هر درخواست کم می‌کند.
    //    برای ویرایش از GetByIdAsync (tracked) استفاده می‌شود.

    public async Task<AllowedIp?> GetByIpAsync(string ipAddress)
        => await Context.AllowedIps
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.IpAddress == ipAddress);

    public async Task<IReadOnlyList<AllowedIp>> GetActiveIpsAsync()
        => await Context.AllowedIps
            .AsNoTracking()
            .Where(a => a.ExpiresAtUtc >= DateTime.UtcNow)
            .ToListAsync();

    public async Task<IReadOnlyList<AllowedIp>> GetAllSortedAsync()
        => await Context.AllowedIps
            .AsNoTracking()
            .OrderBy(a => a.ExpiresAtUtc)
            .ToListAsync();
}
