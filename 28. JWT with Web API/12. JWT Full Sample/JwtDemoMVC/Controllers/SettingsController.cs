// ═══════════════════════════════════════════════════════════════════════════
//  SettingsController — صفحه‌ی تنظیمات IPهای مجاز (ویژگی IP AllowList) — نسخه‌ی بهینه‌شده
//  این صفحه خودش دو لایه محافظت دارد:
//   1) [Authorize] — باید با JWT وارد شده باشید (گام 13)
//   2) Middleware IP AllowList — IP شما باید در جدول AllowedIps فعال باشد
//      (به‌جز اکشن Denied که در ExemptPaths مستثنی شده تا حلقه‌ی ریدایرکت رخ ندهد)
//
//  بهینه‌سازی‌های نسبت به نسخه‌ی قبل (رفتار و مسیرها دقیقاً همان است — Viewها دست‌نخورده می‌مانند):
//   • Primary Constructor (C# 12) — حذف boilerplate تزریق وابستگی
//   • [Bind] روی اکشن Add — جلوگیری از Over-posting (یک مهاجم نتواند فیلدهایی مثل
//     Items یا CurrentIp را از طریق فرم مقداردهی کند)
//   • مدیریت Race Condition ثبت تکراری — حتی اگر دو درخواست همزمان یک IP را ثبت کنند،
//     ایندکس یونیک دیتابیس خطا می‌دهد و ما آن را به پیام دوستانه تبدیل می‌کنیم (نه صفحه‌ی 500)
//   • ثابت‌های نام‌دار برای مقادیر پیش‌فرض و سقف اعتبار — تک‌منبعِ حقیقت
//   • اعتبارسنجی دفاعی محدوده‌ی «مدت اعتبار» داخل کنترلر هم (مستقل از ViewModel)
//   • گارد ورودی در Delete (id نامعتبر → بدون رفتن به دیتابیس رد می‌شود)
//   • لاگ‌گیری (ILogger) برای رخدادهای Add/Delete — قابل ردیابی در کنسول
//   • پاک‌سازی ورودی ip در اکشن Denied — فقط IP معتبر به View می‌رسد (دفاع در عمق)
//   • TempData برای پیام موفقیت — در View فقط یک خط برای نمایشش کافی است (اختیاری)
// ═══════════════════════════════════════════════════════════════════════════

using System.Net;
using JwtDemoMVC.Middleware;
using JwtDemoMVC.Models;
using JwtDemoMVC.Repositories;
using JwtDemoMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Data.SqlClient;

namespace JwtDemoMVC.Controllers;

[Authorize]
[ApiExplorerSettings(IgnoreApi = true)] // این کنترلر View برمی‌گرداند؛ در Swagger نمایش داده نشود
public class SettingsController(
    IUnitOfWork unitOfWork,
    IMemoryCache cache,
    ILogger<SettingsController> logger) : Controller
{
    // ── ثابت‌های نام‌دار: مقادیر پیش‌فرض و سقف اعتبار (مطابق Range در AllowedIpListViewModel) ──
    private const int DefaultValidityMinutes = 60;
    private const int MaxValidityMinutes = 5_256_000; // ۱۰ سال — همان سقف ViewModel

    // GET: /Settings — لیست IPهای مجاز + فرم افزودن
    [HttpGet]
    public async Task<IActionResult> Index()
        => View(await BuildViewModelAsync(new AllowedIpListViewModel()));

    // POST: /Settings/Add — ثبت IP جدید با تاریخ انقضا (ذخیره در جدول AllowedIps)
    // [Bind] فقط فیلدهای فرم را می‌پذیرد → ضد Over-posting؛
    // فیلدهای نمایشی (Items/CurrentIp/...) در BuildViewModelAsync از دیتابیس پر می‌شوند.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(
        [Bind(nameof(AllowedIpListViewModel.NewIp),
              nameof(AllowedIpListViewModel.ValidityMinutes),
              nameof(AllowedIpListViewModel.Note))] AllowedIpListViewModel model)
    {
        if (!ModelState.IsValid)
            return View("Index", await BuildViewModelAsync(model));

        // نرمال‌سازی با همان کدی که Middleware هنگام مقایسه استفاده می‌کند
        // تا چیزی که ذخیره می‌شود دقیقاً با چیزی که هنگام مقایسه ساخته می‌شود یکی باشد.
        if (!IpAllowListMiddleware.TryNormalize(model.NewIp, out var normalized))
        {
            ModelState.AddModelError(nameof(model.NewIp), "آدرس IP معتبر نیست (مثال: 192.168.1.10 یا ::1)");
            return View("Index", await BuildViewModelAsync(model));
        }

        // جلوگیری از ثبت تکراری (ایندکس یونیک هم روی ستون IpAddress هست)
        if (await unitOfWork.AllowedIps.GetByIpAsync(normalized) is not null)
        {
            ModelState.AddModelError(nameof(model.NewIp), "این IP قبلاً ثبت شده است.");
            return View("Index", await BuildViewModelAsync(model));
        }

        // اعتبارسنجی دفاعی محدوده — حتی اگر ViewModel بعداً تغییر کند، کنترلر خودش محافظت دارد
        var minutes = model.ValidityMinutes ?? DefaultValidityMinutes;
        if (minutes is < 1 or > MaxValidityMinutes)
        {
            ModelState.AddModelError(nameof(model.ValidityMinutes),
                $"مدت اعتبار باید بین ۱ تا {MaxValidityMinutes:N0} دقیقه باشد.");
            return View("Index", await BuildViewModelAsync(model));
        }

        var entity = new AllowedIp
        {
            IpAddress = normalized,
            Note = string.IsNullOrWhiteSpace(model.Note) ? null : model.Note.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(minutes), // تاریخ اتمام اعتبار
        };

        try
        {
            await unitOfWork.AllowedIps.AddAsync(entity);
            await unitOfWork.SaveChangesAsync();
        }
        // ═══ مدیریت Race Condition ═══
        // بین چک تکراری بالا و Insert، ممکن است درخواست موازیِ دیگری همان IP را ثبت کرده باشد؛
        // در این حالت ایندکس یونیک دیتابیس (SqlException 2601/2627) خطا می‌دهد.
        // به‌جای صفحه‌ی 500، همان پیام دوستانه‌ی «تکراری» را نشان می‌دهیم.
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            logger.LogWarning("ثبت تکراری IP مجاز به دلیل درخواست همزمان: {Ip}", normalized);
            ModelState.AddModelError(nameof(model.NewIp), "این IP قبلاً ثبت شده است.");
            return View("Index", await BuildViewModelAsync(model));
        }

        InvalidateCache();
        logger.LogInformation("IP مجاز جدید ثبت شد: {Ip} با اعتبار {Minutes} دقیقه", normalized, minutes);
        TempData["Success"] = $"IP {normalized} با موفقیت ثبت شد.";
        return RedirectToAction(nameof(Index)); // الگوی Post/Redirect/Get
    }

    // POST: /Settings/Delete — حذف رکورد (بلافاصله بعد از آن، آن IP رد می‌شود)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        // گارد ورودی: id نامعتبر → بدون رفتن به دیتابیس رد شود
        if (id < 1)
        {
            logger.LogWarning("تلاش برای حذف IP با شناسه‌ی نامعتبر: {Id}", id);
            return RedirectToAction(nameof(Index));
        }

        var item = await unitOfWork.AllowedIps.GetByIdAsync(id);
        if (item is not null)
        {
            unitOfWork.AllowedIps.Remove(item);
            await unitOfWork.SaveChangesAsync();
            InvalidateCache();
            logger.LogInformation("IP مجاز حذف شد: {Ip} (شناسه {Id})", item.IpAddress, id);
            TempData["Success"] = $"IP {item.IpAddress} حذف شد.";
        }

        // حذفِ رکورد ناموجود هم خطا نیست — رفتار Idempotent برای مقاومت در برابر دوباره‌کلیک
        return RedirectToAction(nameof(Index));
    }

    // GET: /Settings/Denied — صفحه‌ی «دسترسی رد شد؛ IP شما مجاز نیست»
    // عمداً [AllowAnonymous] و عمداً در ExemptPaths Middleware مستثنی است:
    // کاربرِ IP نامجاز باید بتواند این صفحه را ببیند، وگرنه ریدایرکت به خودش حلقه می‌ساخت.
    // بهینه‌سازی امنیتی: فقط مقداری که واقعاً IP معتبر باشد به View پاس داده می‌شود
    // (دفاع در عمق — حتی اگر View فراموش کند HTML-Encode کند) + نمایش یکدست نرمال‌شده.
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Denied(string? ip)
    {
        var safeIp = IPAddress.TryParse(ip, out var parsed)
            ? IpAllowListMiddleware.NormalizeIp(parsed)
            : "-";
        return View("Denied", safeIp);
    }

    // ────────────────────────── Helpers ──────────────────────────

    private async Task<AllowedIpListViewModel> BuildViewModelAsync(AllowedIpListViewModel model)
    {
        model.Items = await unitOfWork.AllowedIps.GetAllSortedAsync();
        model.CurrentIp = IpAllowListMiddleware.NormalizeIp(HttpContext.Connection.RemoteIpAddress);
        model.CurrentIpAllowed = await unitOfWork.AllowedIps.GetByIpAsync(model.CurrentIp) is { IsActive: true };
        return model;
    }

    // پس از هر تغییر، کش Middleware بلافاصله باطل می‌شود تا تغییر بدون تأخیر اعمال شود
    private void InvalidateCache() => cache.Remove(IpAllowListMiddleware.CacheKey);
}
