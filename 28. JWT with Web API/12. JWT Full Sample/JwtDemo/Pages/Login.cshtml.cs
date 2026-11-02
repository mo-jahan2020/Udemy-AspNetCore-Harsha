using System.ComponentModel.DataAnnotations;
using JwtDemo.Repositories;
using JwtDemo.Security;
using JwtDemo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JwtDemo.Pages;

// صفحه‌ی ورود برای همه باز است (ویژگی از پکیج Microsoft.AspNetCore.Authorization)
[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;

    public LoginModel(IUnitOfWork unitOfWork, IAuthService authService)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
    }

    [BindProperty]
    [Required(ErrorMessage = "نام کاربری الزامی است")]
    [Display(Name = "نام کاربری")]
    public string Username { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "رمز عبور الزامی است")]
    [DataType(DataType.Password)]
    [Display(Name = "رمز عبور")]
    public string Password { get; set; } = string.Empty;

    public IActionResult OnGet()
        => User.Identity?.IsAuthenticated == true ? RedirectToPage("/Persons") : Page();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var user = await _unitOfWork.Users.GetByUsernameAsync(Username);

        // پیام خطا عمداً کلی است تا مهاجم متوجه نشود کدام فیلد اشتباه بوده است
        if (user is null || !PasswordHasher.Verify(Password, user.PasswordSalt, user.PasswordHash))
        {
            ModelState.AddModelError(string.Empty, "نام کاربری یا رمز عبور اشتباه است.");
            return Page();
        }

        // ورود موفق: صدور جفت توکن (دسترسی + تمدید) و تنظیم کوکی‌ها
        // — گام‌های 17 و 18 در AuthService
        await _authService.IssueTokensAsync(HttpContext, user);

        return RedirectToPage("/Persons");
    }
}
