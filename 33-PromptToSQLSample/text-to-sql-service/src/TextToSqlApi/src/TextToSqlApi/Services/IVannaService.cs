namespace TextToSqlApi.Services;

/// <summary>
/// اینترفیس سرویس Vanna که به میکروسرویس Python متصل می‌شود.
/// </summary>
public interface IVannaService
{
    /// <summary>بررسی سلامت میکروسرویس Python.</summary>
    Task<bool> IsHealthyAsync(CancellationToken ct = default);

    /// <summary>ثبت/به‌روزرسانی شما دیتابیس در Vanna برای آموزش.</summary>
    Task TrainAsync(string schemaText, CancellationToken ct = default);

    /// <summary>تولید SQL از پرامپت کاربر با استفاده از Vanna.</summary>
    Task<(string sql, long elapsedMs)> GenerateSqlAsync(string prompt, CancellationToken ct = default);
}
