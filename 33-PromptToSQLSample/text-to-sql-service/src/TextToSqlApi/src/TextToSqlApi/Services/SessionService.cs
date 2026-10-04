using Microsoft.Extensions.Logging;
using TextToSqlApi.Models.DTOs;
using TextToSqlApi.Models.Entities;
using TextToSqlApi.Repositories;

namespace TextToSqlApi.Services;

/// <summary>
/// پیاده‌سازی ISessionService با هماهنگی Repository.
/// </summary>
public class SessionService : ISessionService
{
    private readonly ISessionRepository _repo;
    private readonly ILogger<SessionService> _logger;

    /// <summary>سازنده با تزریق Repository و Logger.</summary>
    public SessionService(ISessionRepository repo, ILogger<SessionService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<SessionDto> CreateSessionAsync(CreateSessionRequest req, CancellationToken ct = default)
    {
        var session = await _repo.CreateSessionAsync(req.UserName, req.Title, ct);
        return SessionDto.FromEntity(session, 0);
    }

    /// <inheritdoc/>
    public async Task<SessionHistoryDto?> GetSessionHistoryAsync(Guid id, CancellationToken ct = default)
    {
        var session = await _repo.GetSessionAsync(id, includeMessages: true, ct);
        if (session == null) return null;

        var messages = session.Messages
            .OrderBy(m => m.CreatedAt)
            .Select(SessionMessageDto.FromEntity)
            .ToList();

        return new SessionHistoryDto
        {
            Session = SessionDto.FromEntity(session, messages.Count),
            Messages = messages
        };
    }

    /// <inheritdoc/>
    public async Task<List<SessionDto>> GetSessionsByUserAsync(string userName, CancellationToken ct = default)
    {
        var sessions = await _repo.GetSessionsByUserAsync(userName, ct);
        return sessions.Select(s => SessionDto.FromEntity(s, s.Messages.Count)).ToList();
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteSessionAsync(Guid id, CancellationToken ct = default)
    {
        return await _repo.DeleteSessionAsync(id, ct);
    }

    /// <inheritdoc/>
    public async Task<SessionMessageDto> AddUserMessageAsync(Guid sessionId, string prompt, CancellationToken ct = default)
    {
        if (!await _repo.ExistsAsync(sessionId, ct))
            throw new KeyNotFoundException($"Session {sessionId} not found.");

        var msg = new SessionMessage
        {
            SessionId = sessionId,
            Role = MessageRole.User,
            Content = prompt
        };
        msg = await _repo.AddMessageAsync(msg, ct);
        return SessionMessageDto.FromEntity(msg);
    }

    /// <inheritdoc/>
    public async Task<SessionMessageDto> AddAssistantMessageAsync(
        Guid sessionId, string content, string? generatedSql, string? modelName, long? responseTimeMs, CancellationToken ct = default)
    {
        if (!await _repo.ExistsAsync(sessionId, ct))
            throw new KeyNotFoundException($"Session {sessionId} not found.");

        var msg = new SessionMessage
        {
            SessionId = sessionId,
            Role = MessageRole.Assistant,
            Content = content,
            GeneratedSql = generatedSql,
            ModelName = modelName,
            ResponseTimeMs = responseTimeMs
        };
        msg = await _repo.AddMessageAsync(msg, ct);
        return SessionMessageDto.FromEntity(msg);
    }
}
