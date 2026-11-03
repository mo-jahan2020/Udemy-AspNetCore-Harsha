// ═══════════════════════════════════════════════════════════════════════════
//  SettingsController — صفحه‌ی تنظیمات IPهای مجاز (ویژگی IP AllowList)
//  این صفحه خودش دو لایه محافظت دارد:
//   1) [Authorize] — باید با JWT وارد شده باشید (گام 13)
//   2) Middleware IP AllowList — IP شما باید در جدول AllowedIps فعال باشد
//      (به‌جز اکشن Denied که در ExemptPaths مستثنی شده تا حلقه‌ی ریدایرکت رخ ندهد)
// ═══════════════════════════════════════════════════════════════════════════

using JwtDemoMVC.Middleware;
using JwtDemoMVC.Models;
using JwtDemoMVC.Repositories;
using JwtDemoMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace JwtDemoMVC.Controllers;

[Authorize]
[ApiExplorerSettings(IgnoreApi = true)] // این کنترلر View برمی‌گرداند؛ در Swagger نمایش داده نشود
public class SettingsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;

    public SettingsController(IUnitOfWork unitOfWork, IMemoryCache cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    // GET: /Settings — لیست IPهای مجاز + فرم افزودن
    [HttpGet]
    public async Task<IActionResult> Index()
        => View(await BuildViewModelAsync(new AllowedIpListViewModel()));

    // POST: /Settings/Add — ثبت IP جدید با تاریخ انقضا (ذخیره در جدول AllowedIps)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(AllowedIpListViewModel model)
    {
        if (!ModelState.IsValid)
            return View("Index", await BuildViewModelAsync(model));

        // نرمال‌سازی با همان کدی که Middleware هنگام مقایسه استفاده می‌کند
        if (!IpAllowListMiddleware.TryNormalize(model.NewIp, out var normalized))
        {
            ModelState.AddModelError(nameof(model.NewIp), "آدرس IP معتبر نیست (مثال: 192.168.1.10 یا ::1)");
            return View("Index", await BuildViewModelAsync(model));
        }

        // جلوگیری از ثبت تکراری (اینستکس یونیک هم روی ستون IpAddress هست)
        if (await _unitOfWork.AllowedIps.GetByIpAsync(normalized) is not null)
        {
            ModelState.AddModelError(nameof(model.NewIp), "این IP قبلاً ثبت شده است.");
            return View("Index", await BuildViewModelAsync(model));
        }

        var minutes = model.ValidityMinutes ?? 60;
        await _unitOfWork.AllowedIps.AddAsync(new AllowedIp
        {
            IpAddress = normalized,
            Note = string.IsNullOrWhiteSpace(model.Note) ? null : model.Note.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(minutes), // تاریخ اتمام اعتبار
        });
        await _unitOfWork.SaveChangesAsync();

        InvalidateCache();
        return RedirectToAction(nameof(Index)); // الگوی Post/Redirect/Get
    }

    // POST: /Settings/Delete — حذف رکورد (بلافاصله بعد از آن، آن IP رد می‌شود)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _unitOfWork.AllowedIps.GetByIdAsync(id);
        if (item is not null)
        {
            _unitOfWork.AllowedIps.Remove(item);
            await _unitOfWork.SaveChangesAsync();
            InvalidateCache();
        }
        return RedirectToAction(nameof(Index));
    }

    // GET: /Settings/Denied — صفحه‌ی «دسترسی رد شد؛ IP شما مجاز نیست»
    // عمداً [AllowAnonymous] و عمداً در ExemptPaths Middleware مستثنی است:
    // کاربرِ IP نامجاز باید بتواند این صفحه را ببیند، وگرنه ریدایرکت به خودش حلقه می‌ساخت.
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Denied(string? ip)
        => View("Denied", ip);

    // ────────────────────────── Helpers ──────────────────────────

    private async Task<AllowedIpListViewModel> BuildViewModelAsync(AllowedIpListViewModel model)
    {
        model.Items = await _unitOfWork.AllowedIps.GetAllSortedAsync();
        model.CurrentIp = IpAllowListMiddleware.NormalizeIp(HttpContext.Connection.RemoteIpAddress);
        model.CurrentIpAllowed = await _unitOfWork.AllowedIps.GetByIpAsync(model.CurrentIp) is { IsActive: true };
        return model;
    }

    // پس از هر تغییر، کش Middleware بلافاصله باطل می‌شود تا تغییر بدون تأخیر اعمال شود
    private void InvalidateCache() => _cache.Remove(IpAllowListMiddleware.CacheKey);
}
