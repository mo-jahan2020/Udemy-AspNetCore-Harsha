using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace JwtDemoMVC.ViewModels;

/// <summary>مدل فرم ورود (رابط کاربری MVC).</summary>
public class LoginViewModel
{
    [Required(ErrorMessage = "نام کاربری الزامی است")]
    [Display(Name = "نام کاربری")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "رمز عبور الزامی است")]
    [DataType(DataType.Password)]
    [Display(Name = "رمز عبور")]
    public string Password { get; set; } = string.Empty;
}
