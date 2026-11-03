using JwtDemoMVC.Models;
using JwtDemoMVC.Repositories;
using JwtDemoMVC.Security;
using JwtDemoMVC.Services;
using JwtDemoMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JwtDemoMVC.Controllers;

// کنترلر حساب کاربری — ورود، ثبت‌نام و خروج (رابط کاربری MVC)
// این کنترلر View برمی‌گرداند؛ در Swagger نمایش داده نشود
[ApiExplorerSettings(IgnoreApi = true)]
public class AccountController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;

    public AccountController(IUnitOfWork unitOfWork, IAuthService authService)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
    }

    // ───────────────────────────── ورود ─────────────────────────────
    // همیشه اول لاگین اجرا میشود کاربر قبلاً ورود کرده بود به صفحه /person منتقل میشود . وگرنه در همین login باقی میماند
    [HttpGet]
    [AllowAnonymous] // صفحه‌ی ورود برای همه باز است (ویژگی از پکیج Microsoft.AspNetCore.Authorization)
    public IActionResult Login()
        => User.Identity?.IsAuthenticated == true
            ? RedirectToAction("Index", "Person")
            : View(new LoginViewModel());

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _unitOfWork.Users.GetByUsernameAsync(model.Username);

        // پیام خطا عمداً کلی است تا مهاجم متوجه نشود کدام فیلد اشتباه بوده است
        if (user is null || !PasswordHasher.Verify(model.Password, user.PasswordSalt, user.PasswordHash))
        {
            ModelState.AddModelError(string.Empty, "نام کاربری یا رمز عبور اشتباه است.");
            return View(model);
        }

        // ورود موفق: صدور جفت توکن (دسترسی + تمدید) و تنظیم کوکی‌ها
        // — گام‌های 17 و 18 در AuthService
        await _authService.IssueTokensAsync(HttpContext, user);

        return RedirectToAction("Index", "Person");
    }

    // ───────────────────────────── ثبت‌نام ─────────────────────────────

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
        => User.Identity?.IsAuthenticated == true
            ? RedirectToAction("Index", "Person")
            : View(new RegisterViewModel());

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // یکتا بودن نام کاربری
        if (await _unitOfWork.Users.GetByUsernameAsync(model.Username) is not null)
        {
            ModelState.AddModelError(nameof(model.Username), "این نام کاربری قبلاً ثبت شده است.");
            return View(model);
        }

        // ساخت کاربر با هش رمز عبور (این بخش جزو پیاده‌سازی JWT نیست)
        var (hash, salt) = PasswordHasher.Hash(model.Password);
        var user = new User
        {
            Username = model.Username.Trim(),
            PasswordHash = hash,
            PasswordSalt = salt,
        };
        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // ثبت‌نام موفق → ورود خودکار: صدور جفت توکن و تنظیم کوکی‌ها — گام‌های 17 و 18
        await _authService.IssueTokensAsync(HttpContext, user);

        return RedirectToAction("Index", "Person");
    }

    // ───────────────────────────── خروج ─────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        // گام 22 (در AuthService): ابطال توکن تمدید در دیتابیس + حذف کوکی‌ها.
        // توکن دسترسی 1 دقیقه‌ای بعد از این دیگر قابل تمدید نیست و مسیر کاربر بسته می‌شود.
        await _authService.RevokeAsync(HttpContext);

        return RedirectToAction(nameof(Login));
    }

    // حالت GET هم (برای راحتی) خروج انجام می‌دهد
    [HttpGet]
    public async Task<IActionResult> LogoutGet()
    {
        await _authService.RevokeAsync(HttpContext);
        return RedirectToAction(nameof(Login));
    }
}
