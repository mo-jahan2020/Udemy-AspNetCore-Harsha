using System.Security.Claims;
using AsvsLab.Models;
using AsvsLab.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AsvsLab.Controllers;

// ═══════════════════════════════════════════════════════════════════
//  کنترلر احراز هویت واقعی (امکان جدید نسخه ۲)
//  ورود/ثبت‌نام/خروج/پروفایل با کوکی احراز هویت و نقش‌ها
//
//  بندهای مرتبط ASVS 5.0:
//    V6.2.2 / V6.2.3 — تغییر گذرواژه و جریان احراز هویت
//    V6.2.4 / V6.2.12 — بررسی گذرواژه در برابر فهرست لو رفته در ثبت‌نام
//    V6.3.1 — قفل‌گذاری پس از تلاش‌های ناموفق (ضد Brute Force)
//    V7.2.4 — تولید توکن سشن جدید پس از ورود (ضد Session Fixation)
//    V7.4.1 — ابطال کامل سشن پس از خروج
//    V3.7.2 — بازگشت فقط به آدرس داخلی (returnUrl)
// ═══════════════════════════════════════════════════════════════════
public class AccountController : Controller
{
    private readonly DemoUserStore _store;
    private readonly PasswordHasher _hasher;
    private readonly InMemoryLockoutService _lockout;
    private readonly ILogger<AccountController> _logger;

    public AccountController(DemoUserStore store, PasswordHasher hasher,
        InMemoryLockoutService lockout, ILogger<AccountController> logger)
    {
        _store = store;
        _hasher = hasher;
        _lockout = lockout;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl) =>
        View(new LoginViewModel { /* راهنمای کاربران دمو در ویو نمایش داده می‌شود */ });

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken] // بند V3 — ضد CSRF در فرم‌های تغییر وضعیت
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl)
    {
        var lockKey = $"{model.Username}|{HttpContext.Connection.RemoteIpAddress}";

        // ─── بند V6.3.1 — بررسی قفل‌گذاری پیش از هر بررسی دیگری ───
        if (_lockout.IsLockedOut(lockKey, out var remaining))
        {
            // بند V6.3.8 — پیام خطا نباید نشان دهد حساب وجود دارد یا نه
            model.Error = $"به دلیل تلاش‌های ناموفق زیاد، ورود موقتاً قفل شد. حدود {remaining} دقیقه دیگر دوباره تلاش کنید.";
            return View(model);
        }

        var user = _store.FindByName(model.Username);
        if (user is null || !_hasher.Verify(model.Password, user.PasswordHash))
        {
            var locked = _lockout.RegisterFailure(lockKey);
            // بند V16.3.2 — ثبت لاگ تلاش ناموفق احراز هویت (بدون داده حساس)
            _logger.LogWarning("تلاش ورود ناموفق برای کاربر {User}", model.Username);
            model.Error = locked
                ? "حساب شما به دلیل ۵ تلاش ناموفق برای ۱۰ دقیقه قفل شد."
                : "نام کاربری یا گذرواژه نادرست است.";
            return View(model);
        }

        _lockout.Reset(lockKey); // ورود موفق → بازنشانی شمارنده

        // ─── بند V7.2.4 — سشن قدیمی پاک می‌شود تا توکن جدیدی صادر شود (ضد Session Fixation) ───
        HttpContext.Session.Clear();

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Username),
            new("FullName", user.FullName),
            new(ClaimTypes.Role, user.Role),   // بند V8.2.1 — نقش برای کنترل دسترسی سطح تابع
            new("UserId", user.Id.ToString()),
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = false, ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30) });

        // ─── بند V3.7.2 — بازگشت فقط به آدرس داخلی برنامه (ضد Open Redirect) ───
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public IActionResult Register(RegisterViewModel model)
    {
        // ─── بند V6.2.12 / V6.2.11 — بررسی سیاست گذرواژه در ثبت‌نام ───
        var checks = PasswordPolicy.Evaluate(model.Password);
        model.PolicyErrors = checks.Where(c => !c.Passed && c.Ref != "V6.2.9")
                                   .Select(c => $"{c.Ref}: {c.Detail}").ToList();

        if (_store.Exists(model.Username))
            model.PolicyErrors.Add("این نام کاربری قبلاً ثبت شده است.");

        if (model.PolicyErrors.Count > 0 || !ModelState.IsValid)
            return View(model);

        // ─── بند V11.4.2 — هش PBKDF2 ذخیره می‌شود، هرگز متن آشکار ───
        var user = _store.Register(model.Username, model.Password, model.FullName, _hasher);
        _logger.LogInformation("کاربر جدید با شناسه داخلی {Id} ثبت‌نام کرد", user.Id); // بدون داده حساس

        // بند V7.2.4 — سشن تازه
        HttpContext.Session.Clear();
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Username),
            new("FullName", user.FullName),
            new(ClaimTypes.Role, user.Role),
            new("UserId", user.Id.ToString()),
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return SignIn(new ClaimsPrincipal(identity), CookieAuthenticationDefaults.AuthenticationScheme);
    }

    [Authorize] // بند V8.2.1 — بدون احراز هویت، دسترسی رد می‌شود
    public IActionResult Profile()
    {
        var name = User.Identity?.Name ?? "";
        var user = _store.FindByName(name);
        if (user is null) return RedirectToAction("Logout");
        ViewBag.Invoices = _store.InvoicesOf(user.Id); // فقط داده خود کاربر — بند V8.2.2
        return View(user);
    }

    // ─── بند V8.2.1 — کنترل دسترسی سطح تابع: فقط نقش Admin ───
    [Authorize(Roles = "Admin")]
    public IActionResult AdminPanel()
    {
        ViewBag.TotalUsers = _store.TotalUsers();
        return View();
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        // ─── بند V7.4.1 — پس از خروج، سشن و کوکی باید کاملاً باطل شوند ───
        HttpContext.Session.Clear();      // پاک‌سازی کامل داده سشن سمت سرور
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        Response.Cookies.Delete("AsvsLab.Auth");
        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    public IActionResult Denied() => View(); // صفحه دسترسی غیرمجاز (403)
}
