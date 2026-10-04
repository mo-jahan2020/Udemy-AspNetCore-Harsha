using TextToSqlApi.Models.DTOs;
using TextToSqlApi.Models.Entities;

namespace TextToSqlApi.Repositories;

/// <summary>
/// اینترفیس Repository برای مدیریت نشست‌ها (Sessions).
/// این لایه ارتباط با دیتابیس را کپسوله می‌کند.
/// </summary>
public interface ISessionRepository
{
    /// <summary>یک نشست جدید ایجاد می‌کند.</summary>
    Task<Session> CreateSessionAsync(string userName, string? title, CancellationToken ct = default);

    /// <summary>یک نشست را بر اساس شناسه برمی‌گرداند (شامل پیام‌ها).</summary>
    Task<Session?> GetSessionAsync(Guid id, bool includeMessages = true, CancellationToken ct = default);

    /// <summary>لیست نشست‌های یک کاربر را برمی‌گرداند.</summary>
    Task<List<Session>> GetSessionsByUserAsync(string userName, CancellationToken ct = default);

    /// <summary>یک نشست را حذف می‌کند (همراه با پیام‌ها به‌صورت Cascade).</summary>
    Task<bool> DeleteSessionAsync(Guid id, CancellationToken ct = default);

    /// <summary>یک پیام جدید به نشست اضافه می‌کند.</summary>
    Task<SessionMessage> AddMessageAsync(SessionMessage message, CancellationToken ct = default);

    /// <summary>پیام‌های یک نشست را به ترتیب زمان برمی‌گرداند.</summary>
    Task<List<SessionMessage>> GetMessagesAsync(Guid sessionId, CancellationToken ct = default);

    /// <summary>بررسی وجود یک نشست.</summary>
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);

    /// <summary>ذخیره تغییرات در دیتابیس.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
