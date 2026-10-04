using TextToSqlApi.Models.DTOs;

namespace TextToSqlApi.Services;

/// <summary>
/// اینترفیس سرویس مدیریت نشست‌ها (Sessions).
/// این لایه Business Logic را کپسوله می‌کند و با Repository کار می‌کند.
/// </summary>
public interface ISessionService
{
    Task<SessionDto> CreateSessionAsync(CreateSessionRequest req, CancellationToken ct = default);
    Task<SessionHistoryDto?> GetSessionHistoryAsync(Guid id, CancellationToken ct = default);
    Task<List<SessionDto>> GetSessionsByUserAsync(string userName, CancellationToken ct = default);
    Task<bool> DeleteSessionAsync(Guid id, CancellationToken ct = default);
    Task<SessionMessageDto> AddUserMessageAsync(Guid sessionId, string prompt, CancellationToken ct = default);
    Task<SessionMessageDto> AddAssistantMessageAsync(Guid sessionId, string content, string? generatedSql, string? modelName, long? responseTimeMs, CancellationToken ct = default);
}
