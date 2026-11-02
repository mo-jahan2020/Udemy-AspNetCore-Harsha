using System.ComponentModel.DataAnnotations;
using JwtDemo.Models;
using JwtDemo.Repositories;
using JwtDemo.Security;
using JwtDemo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JwtDemo.Pages;

// صفحه‌ی ثبت‌نام برای همه باز است (ویژگی از پکیج Microsoft.AspNetCore.Authorization)
[AllowAnonymous]
public class RegisterModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;

    public RegisterModel(IUnitOfWork unitOfWork, IAuthService authService)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
    }

    [BindProperty]
    [Required(ErrorMessage = "نام کاربری الزامی است")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "نام کاربری باید بین 3 تا 50 کاراکتر باشد")]
    [Display(Name = "نام کاربری")]
    public string Username { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "رمز عبور الزامی است")]
    [StringLength(100, MinimumLength = 4, ErrorMessage = "رمز عبور باید حداقل 4 کاراکتر باشد")]
    [DataType(DataType.Password)]
    [Display(Name = "رمز عبور")]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "تکرار رمز عبور الزامی است")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "تکرار رمز عبور با رمز عبور یکسان نیست")]
    [Display(Name = "تکرار رمز عبور")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public IActionResult OnGet()
        => User.Identity?.IsAuthenticated == true ? RedirectToPage("/Persons") : Page();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        // یکتا بودن نام کاربری
        if (await _unitOfWork.Users.GetByUsernameAsync(Username) is not null)
        {
            ModelState.AddModelError(nameof(Username), "این نام کاربری قبلاً ثبت شده است.");
            return Page();
        }

        // ساخت کاربر با هش رمز عبور (این بخش جزو پیاده‌سازی JWT نیست)
        var (hash, salt) = PasswordHasher.Hash(Password);
        var user = new User
        {
            Username = Username.Trim(),
            PasswordHash = hash,
            PasswordSalt = salt,
        };
        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // ثبت‌نام موفق → ورود خودکار: صدور جفت توکن و تنظیم کوکی‌ها — گام‌های 17 و 18
        await _authService.IssueTokensAsync(HttpContext, user);

        return RedirectToPage("/Persons");
    }
}
