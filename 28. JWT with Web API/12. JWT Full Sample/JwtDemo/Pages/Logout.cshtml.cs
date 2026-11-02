using JwtDemo.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JwtDemo.Pages;

// صفحه‌ی خروج — کار عملیاتی خروج در AuthService.RevokeAsync انجام می‌شود (گام 22)
public class LogoutModel : PageModel
{
    private readonly IAuthService _authService;

    public LogoutModel(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<IActionResult> OnGetAsync() => await LogoutAsync();

    // خروج از فرم هدر با POST انجام می‌شود
    public async Task<IActionResult> OnPostAsync() => await LogoutAsync();

    private async Task<IActionResult> LogoutAsync()
    {
        // گام 22 (در AuthService): ابطال توکن تمدید در دیتابیس + حذف کوکی‌ها.
        // توکن دسترسی 1 دقیقه‌ای بعد از این دیگر قابل تمدید نیست و مسیر کاربر بسته می‌شود.
        await _authService.RevokeAsync(HttpContext);

        return RedirectToPage("/Login");
    }
}
