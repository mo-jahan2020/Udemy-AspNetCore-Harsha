using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TextToSqlApi.Models.Entities;

/// <summary>
/// نقش پیام در مکالمه (کاربر یا دستیار).
/// </summary>
public enum MessageRole
{
    User = 0,
    Assistant = 1
}

/// <summary>
/// موجودیت پیام‌های داخل یک Session شامل پرامپت کاربر و SQL تولیدشده.
/// </summary>
public class SessionMessage
{
    /// <summary>شناسه یکتای پیام.</summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>شناسه نشست والد (کلید خارجی).</summary>
    [Required]
    [ForeignKey(nameof(Session))]
    public Guid SessionId { get; set; }

    /// <summary>مرجع به موجودیت Session.</summary>
    public Session? Session { get; set; }

    /// <summary>نقش پیام (User یا Assistant).</summary>
    public MessageRole Role { get; set; } = MessageRole.User;

    /// <summary>متن پرامپت یا پاسخ (Prompt یا SQL تولیدشده).</summary>
    [Required]
    public string Content { get; set; } = string.Empty;

    /// <summary>اگر نقش Assistant باشد، SQL تولیدشده در این فیلد نگه‌داری می‌شود.</summary>
    public string? GeneratedSql { get; set; }

    /// <summary>نام مدل تولیدکننده SQL (اختیاری).</summary>
    [MaxLength(100)]
    public string? ModelName { get; set; }

    /// <summary>زمان ثبت پیام (UTC).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>زمان پاسخ به میلی‌ثانیه (اختیاری، برای تحلیل کارایی).</summary>
    public long? ResponseTimeMs { get; set; }
}
