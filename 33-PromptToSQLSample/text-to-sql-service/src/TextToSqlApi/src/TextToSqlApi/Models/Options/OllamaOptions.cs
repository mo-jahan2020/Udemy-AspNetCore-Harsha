namespace TextToSqlApi.Models.Options;

/// <summary>
/// تنظیمات مربوط به اتصال به سرویس Ollama (مدل محلی).
/// این مقادیر از appsettings.json یا Environment Variables خوانده می‌شوند.
/// </summary>
public class OllamaOptions
{
    public const string SectionName = "Ollama";

    /// <summary>آدرس پایه سرویس Ollama (پیش‌فرض: http://localhost:11434).</summary>
    public string BaseUrl { get; set; } = "http://localhost:11434";

    /// <summary>نام مدل مورد استفاده برای تولید SQL (مثلاً qwen2.5-coder).</summary>
    public string Model { get; set; } = "qwen2.5-coder";

    /// <summary>زمان انتظار درخواست HTTP به Ollama به ثانیه.</summary>
    public int TimeoutSeconds { get; set; } = 120;

    /// <summary>دمای نمونه‌برداری (Sampling Temperature).</summary>
    public double Temperature { get; set; } = 0.2;

    /// <summary>حداکثر توکن‌های تولیدشده.</summary>
    public int MaxTokens { get; set; } = 1024;
}
