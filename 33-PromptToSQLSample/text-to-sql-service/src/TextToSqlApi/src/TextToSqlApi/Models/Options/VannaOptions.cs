namespace TextToSqlApi.Models.Options;

/// <summary>
/// تنظیمات مربوط به میکروسرویس Python (Vanna + FastAPI).
/// </summary>
public class VannaOptions
{
    public const string SectionName = "Vanna";

    /// <summary>آدرس پایه میکروسرویس Vanna (مثلاً http://localhost:8001).</summary>
    public string BaseUrl { get; set; } = "http://localhost:8001";

    /// <summary>زمان انتظار درخواست به سرویس Vanna به ثانیه.</summary>
    public int TimeoutSeconds { get; set; } = 180;

    /// <summary>
    /// اگر true باشد، در صورت در دسترس نبودن Vanna، درخواست به‌صورت مستقیم به Ollama ارسال می‌شود.
    /// </summary>
    public bool EnableFallbackToOllama { get; set; } = true;
}
