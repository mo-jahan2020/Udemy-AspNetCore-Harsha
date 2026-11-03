namespace JwtDemoMVC.Models;

/// <summary>
/// کاربر برنامه — به صورت دستی و ساده (بدون ASP.NET Identity).
/// هویت کاربر بعداً داخل توکن JWT به شکل Claims قرار می‌گیرد (گام 6 در TokenService.cs).
/// </summary>
public class User
{
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    // هش BCrypt رمز عبور — نمک و Work Factor «داخل خود رشته‌ی هش» ذخیره می‌شوند
    // (فرمت $2b$12$...)؛ برخلاف روش قبلی (SHA256) دیگر ستون جداگانه‌ی PasswordSalt لازم نیست.
    public string PasswordHash { get; set; } = string.Empty;

    // توکن‌های تمدید این کاربر — یک کاربر می‌تواند از چند دستگاه وارد شود
    public List<RefreshToken> RefreshTokens { get; set; } = new();
}
