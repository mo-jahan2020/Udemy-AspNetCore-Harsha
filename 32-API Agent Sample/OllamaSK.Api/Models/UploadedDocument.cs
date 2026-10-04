namespace OllamaSK.Api.Models;

/// <summary>سند متنی آپلودشده (txt / docx / xlsx) به‌همراه محتوای استخراج‌شده</summary>
public sealed class UploadedDocument
{
    /// <summary>شناسه یکتای سند</summary>
    public Guid Id { get; init; }

    /// <summary>نام اصلی فایل</summary>
    public string FileName { get; init; } = string.Empty;

    /// <summary>نوع فایل: Text | Word | Excel</summary>
    public string FileKind { get; init; } = "Text";

    /// <summary>حجم فایل به بایت</summary>
    public long SizeBytes { get; init; }

    /// <summary>متن استخراج‌شده از فایل</summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>تعداد کاراکترهای استخراج‌شده</summary>
    public int ContentChars { get; init; }

    /// <summary>زمان آپلود</summary>
    public DateTimeOffset UploadedAt { get; init; }
}
