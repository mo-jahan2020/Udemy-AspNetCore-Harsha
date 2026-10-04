using TextToSqlApi.Models.DTOs;

namespace TextToSqlApi.Services;

/// <summary>
/// اینترفیس سرویس اتصال به Ollama (مدل محلی).
/// </summary>
public interface IOllamaService
{
    /// <summary>بررسی اینکه آیا سرویس Ollama در دسترس است یا خیر.</summary>
    Task<bool> IsHealthyAsync(CancellationToken ct = default);

    /// <summary>
    /// تولید SQL از پرامپت کاربر با تزریق شمای دیتابیس در System Prompt.
    /// </summary>
    Task<(string sql, long elapsedMs)> GenerateSqlAsync(string prompt, string schemaText, CancellationToken ct = default);
}
