using TextToSqlApi.Models.DTOs;

namespace TextToSqlApi.Services;

/// <summary>
/// اینترفیس سرویس SQL برای اتصال به دیتابیس هدف کاربر.
/// </summary>
public interface ISqlService
{
    /// <summary>بررسی سلامت اتصال به دیتابیس هدف.</summary>
    Task<bool> IsHealthyAsync(CancellationToken ct = default);

    /// <summary>
    /// اجرای یک Query فقط خواندنی (Read-Only) و بازگشت نتایج.
    /// این متد دستورات مخرب را با Regex و لیست سیاه بلاک می‌کند.
    /// </summary>
    Task<ExecuteSqlResponse> ExecuteReadOnlyAsync(string sql, int maxRows = 100, CancellationToken ct = default);
}
