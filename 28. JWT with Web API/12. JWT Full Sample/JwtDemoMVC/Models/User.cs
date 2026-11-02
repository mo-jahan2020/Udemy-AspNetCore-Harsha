namespace JwtDemoMVC.Models;

/// <summary>
/// کاربر برنامه — به صورت دستی و ساده (بدون ASP.NET Identity).
/// هویت کاربر بعداً داخل توکن JWT به شکل Claims قرار می‌گیرد (گام 6 در TokenService.cs).
/// </summary>
public class User
{
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    // هش و نمک رمز عبور (SHA256 + Salt — برای پروژه واقعی از BCrypt/PBKDF2 استفاده کنید)
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;

    // توکن‌های تمدید این کاربر — یک کاربر می‌تواند از چند دستگاه وارد شود
    public List<RefreshToken> RefreshTokens { get; set; } = new();
}
