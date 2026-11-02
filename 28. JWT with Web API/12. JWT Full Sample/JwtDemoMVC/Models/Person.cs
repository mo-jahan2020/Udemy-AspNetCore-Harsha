using System.ComponentModel.DataAnnotations;

namespace JwtDemoMVC.Models;

/// <summary>
/// موجودیت «شخص» — تنها مدل دامنه‌ی برنامه (این بخش جزو پیاده‌سازی JWT نیست).
/// </summary>
public class Person
{
    public int Id { get; set; }

    [Required(ErrorMessage = "نام الزامی است")]
    [MaxLength(50, ErrorMessage = "نام حداکثر ۵۰ کاراکتر است")]
    [Display(Name = "نام")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "نام خانوادگی الزامی است")]
    [MaxLength(50, ErrorMessage = "نام خانوادگی حداکثر ۵۰ کاراکتر است")]
    [Display(Name = "نام خانوادگی")]
    public string LastName { get; set; } = string.Empty;

    [Range(1, 120, ErrorMessage = "سن باید بین ۱ تا ۱۲۰ باشد")]
    [Display(Name = "سن")]
    public int Age { get; set; }
}
