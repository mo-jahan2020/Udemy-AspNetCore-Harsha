using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TextToSqlApi.Data;
using TextToSqlApi.Models.Entities;

namespace TextToSqlApi.Repositories;

/// <summary>
/// پیاده‌سازی ISessionRepository با EF Core روی SQL Server Express.
/// </summary>
public class SessionRepository : ISessionRepository
{
    private readonly SessionDbContext _db;
    private readonly ILogger<SessionRepository> _logger;

    /// <summary>سازنده با تزریق DbContext و Logger.</summary>
    public SessionRepository(SessionDbContext db, ILogger<SessionRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Session> CreateSessionAsync(string userName, string? title, CancellationToken ct = default)
    {
        var session = new Session
        {
            Id = Guid.NewGuid(),
            UserName = userName,
            Title = title,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _db.Sessions.AddAsync(session, ct);
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Session {SessionId} created for user {UserName}", session.Id, userName);
        return session;
    }

    /// <inheritdoc/>
    public async Task<Session?> GetSessionAsync(Guid id, bool includeMessages = true, CancellationToken ct = default)
    {
        var query = _db.Sessions.AsNoTracking();
        if (includeMessages)
            query = query.Include(s => s.Messages);
        return await query.FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    /// <inheritdoc/>
    public async Task<List<Session>> GetSessionsByUserAsync(string userName, CancellationToken ct = default)
    {
        return await _db.Sessions
            .AsNoTracking()
            .Where(s => s.UserName == userName)
            .OrderByDescending(s => s.UpdatedAt)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteSessionAsync(Guid id, CancellationToken ct = default)
    {
        var session = await _db.Sessions.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (session == null) return false;

        _db.Sessions.Remove(session);
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Session {SessionId} deleted", id);
        return true;
    }

    /// <inheritdoc/>
    public async Task<SessionMessage> AddMessageAsync(SessionMessage message, CancellationToken ct = default)
    {
        message.Id = Guid.NewGuid();
        message.CreatedAt = DateTime.UtcNow;
        await _db.SessionMessages.AddAsync(message, ct);

        // به‌روزرسانی UpdatedAt نشست والد
        var session = await _db.Sessions.FirstOrDefaultAsync(s => s.Id == message.SessionId, ct);
        if (session != null)
        {
            session.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct);
        return message;
    }

    /// <inheritdoc/>
    public async Task<List<SessionMessage>> GetMessagesAsync(Guid sessionId, CancellationToken ct = default)
    {
        return await _db.SessionMessages
            .AsNoTracking()
            .Where(m => m.SessionId == sessionId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Sessions.AnyAsync(s => s.Id == id, ct);
    }

    /// <inheritdoc/>
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _db.SaveChangesAsync(ct);
    }
}
