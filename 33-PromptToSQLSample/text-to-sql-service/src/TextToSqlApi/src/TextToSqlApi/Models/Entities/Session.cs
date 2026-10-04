using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TextToSqlApi.Models.Entities;

/// <summary>
/// موجودیت Session که یک نشست چت کاربر را نمایندگی می‌کند.
/// </summary>
public class Session
{
    /// <summary>شناسه یکتای نشست.</summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>نام کاربر صاحب نشست.</summary>
    [Required]
    [MaxLength(200)]
    public string UserName { get; set; } = string.Empty;

    /// <summary>عنوان اختیاری نشست.</summary>
    [MaxLength(500)]
    public string? Title { get; set; }

    /// <summary>زمان ایجاد نشست (UTC).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>زمان آخرین به‌روزرسانی نشست (UTC).</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>لیست پیام‌های این نشست (Navigation Property).</summary>
    public ICollection<SessionMessage> Messages { get; set; } = new List<SessionMessage>();
}
