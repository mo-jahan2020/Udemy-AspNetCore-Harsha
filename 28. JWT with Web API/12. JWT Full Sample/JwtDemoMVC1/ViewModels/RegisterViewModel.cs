using System.ComponentModel.DataAnnotations;

namespace JwtDemoMVC.ViewModels;

/// <summary>مدل فرم ثبت‌نام (رابط کاربری MVC).</summary>
public class RegisterViewModel
{
    [Required(ErrorMessage = "نام کاربری الزامی است")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "نام کاربری باید بین 3 تا 50 کاراکتر باشد")]
    [Display(Name = "نام کاربری")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "رمز عبور الزامی است")]
    [StringLength(100, MinimumLength = 4, ErrorMessage = "رمز عبور باید حداقل 4 کاراکتر باشد")]
    [DataType(DataType.Password)]
    [Display(Name = "رمز عبور")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "تکرار رمز عبور الزامی است")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "تکرار رمز عبور با رمز عبور یکسان نیست")]
    [Display(Name = "تکرار رمز عبور")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
