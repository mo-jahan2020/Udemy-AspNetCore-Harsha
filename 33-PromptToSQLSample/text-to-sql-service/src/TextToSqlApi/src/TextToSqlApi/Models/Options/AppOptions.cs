namespace TextToSqlApi.Models.Options;

/// <summary>
/// تنظیمات اجرایی سرویس (پورت، مسیرهای Health Check و ...).
/// </summary>
public class AppOptions
{
    public const string SectionName = "App";

    /// <summary>پورتی که Web API روی آن listen می‌کند.</summary>
    public int Port { get; set; } = 5000;

    /// <summary>آدرس‌های مجاز برای CORS (با کاما جدا شوند).</summary>
    public string[] CorsOrigins { get; set; } = Array.Empty<string>();

    /// <summary>اگر true باشد، دستورات اجرای مستقیم SQL مجاز است.</summary>
    public bool EnableSqlExecution { get; set; } = true;
}
