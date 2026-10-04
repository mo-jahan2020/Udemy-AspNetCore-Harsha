using System.ComponentModel.DataAnnotations;
using TextToSqlApi.Models.Entities;

namespace TextToSqlApi.Models.DTOs;

/// <summary>درخواست ایجاد نشست جدید.</summary>
public class CreateSessionRequest
{
    /// <summary>نام کاربر صاحب نشست.</summary>
    [Required, MaxLength(200)]
    public string UserName { get; set; } = string.Empty;

    /// <summary>عنوان اختیاری نشست.</summary>
    [MaxLength(500)]
    public string? Title { get; set; }
}

/// <summary>مدل خروجی نشست.</summary>
public class SessionDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? Title { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int MessageCount { get; set; }

    /// <summary>تبدیل Entity به DTO.</summary>
    public static SessionDto FromEntity(Session s, int? messageCount = null) => new()
    {
        Id = s.Id,
        UserName = s.UserName,
        Title = s.Title,
        CreatedAt = s.CreatedAt,
        UpdatedAt = s.UpdatedAt,
        MessageCount = messageCount ?? s.Messages.Count
    };
}

/// <summary>مدل خروجی پیام داخل نشست.</summary>
public class SessionMessageDto
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public MessageRole Role { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? GeneratedSql { get; set; }
    public string? ModelName { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? ResponseTimeMs { get; set; }

    public static SessionMessageDto FromEntity(SessionMessage m) => new()
    {
        Id = m.Id,
        SessionId = m.SessionId,
        Role = m.Role,
        Content = m.Content,
        GeneratedSql = m.GeneratedSql,
        ModelName = m.ModelName,
        CreatedAt = m.CreatedAt,
        ResponseTimeMs = m.ResponseTimeMs
    };
}

/// <summary>تاریخچه کامل یک نشست به همراه پیام‌ها.</summary>
public class SessionHistoryDto
{
    public SessionDto Session { get; set; } = new();
    public List<SessionMessageDto> Messages { get; set; } = new();
}
