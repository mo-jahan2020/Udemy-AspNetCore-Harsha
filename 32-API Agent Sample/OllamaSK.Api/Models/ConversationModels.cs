namespace OllamaSK.Api.Models;

/// <summary>خلاصه یک مکالمه برای نمایش در سایدبار</summary>
public sealed record ConversationInfo(
    Guid Id,
    string Title,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    int MessageCount);

/// <summary>یک پیام ذخیره‌شده در تاریخچه (کاربر یا دستیار)</summary>
public sealed record ConversationMessage(
    long Id,
    string Role,
    string Content,
    string? Thinking,
    DateTimeOffset CreatedAt);

/// <summary>درخواست ساخت مکالمه جدید (تیتر اختیاری است)</summary>
public sealed record CreateConversationRequest(string? Title = null);

/// <summary>درخواست تغییر عنوان مکالمه</summary>
public sealed record RenameConversationRequest(string Title);
