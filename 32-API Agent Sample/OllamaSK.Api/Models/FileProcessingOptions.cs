namespace OllamaSK.Api.Models;

/// <summary>تنظیمات پردازش فایل — از بخش «FileProcessing» در appsettings.json خوانده می‌شود</summary>
public sealed class FileProcessingOptions
{
    /// <summary>حداکثر حجم مجاز هر فایل (مگابایت)</summary>
    public int MaxFileSizeMB { get; set; } = 10;

    /// <summary>حداکثر کاراکتر استخراجی از هر سند تا کانتکست مدل پر نشود</summary>
    public int MaxCharsPerDocument { get; set; } = 12000;
}
