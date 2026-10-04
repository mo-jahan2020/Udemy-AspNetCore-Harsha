using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TextToSqlApi.Data;
using TextToSqlApi.Models.Entities;
using TextToSqlApi.Repositories;
using Xunit;

namespace TextToSqlApi.Tests.Repositories;

/// <summary>تست‌های SessionRepository با EF Core InMemory.</summary>
public class SessionRepositoryTests
{
    private static SessionDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SessionDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new SessionDbContext(options);
    }

    [Fact]
    public async Task CreateSessionAsync_Saves_Session()
    {
        using var ctx = CreateContext();
        var repo = new SessionRepository(ctx, NullLogger<SessionRepository>.Instance);

        var session = await repo.CreateSessionAsync("alice", "Test Session");

        Assert.NotEqual(Guid.Empty, session.Id);
        Assert.Equal("alice", session.UserName);
        Assert.Equal("Test Session", session.Title);
        Assert.Single(ctx.Sessions);
    }

    [Fact]
    public async Task GetSessionAsync_Returns_Session_With_Messages()
    {
        using var ctx = CreateContext();
        var repo = new SessionRepository(ctx, NullLogger<SessionRepository>.Instance);

        var session = await repo.CreateSessionAsync("bob", null);
        await repo.AddMessageAsync(new SessionMessage
        {
            SessionId = session.Id,
            Role = MessageRole.User,
            Content = "Hello"
        });
        await repo.AddMessageAsync(new SessionMessage
        {
            SessionId = session.Id,
            Role = MessageRole.Assistant,
            Content = "SELECT 1",
            GeneratedSql = "SELECT 1"
        });

        var fetched = await repo.GetSessionAsync(session.Id, includeMessages: true);

        Assert.NotNull(fetched);
        Assert.Equal(2, fetched!.Messages.Count);
    }

    [Fact]
    public async Task DeleteSessionAsync_Removes_Session_And_Messages()
    {
        using var ctx = CreateContext();
        var repo = new SessionRepository(ctx, NullLogger<SessionRepository>.Instance);

        var session = await repo.CreateSessionAsync("carol", null);
        await repo.AddMessageAsync(new SessionMessage
        {
            SessionId = session.Id,
            Role = MessageRole.User,
            Content = "Hi"
        });

        var deleted = await repo.DeleteSessionAsync(session.Id);

        Assert.True(deleted);
        Assert.False(await repo.ExistsAsync(session.Id));
        Assert.Empty(ctx.Sessions);
        Assert.Empty(ctx.SessionMessages);
    }

    [Fact]
    public async Task GetSessionsByUserAsync_Returns_Only_User_Sessions()
    {
        using var ctx = CreateContext();
        var repo = new SessionRepository(ctx, NullLogger<SessionRepository>.Instance);

        await repo.CreateSessionAsync("alice", "S1");
        await repo.CreateSessionAsync("alice", "S2");
        await repo.CreateSessionAsync("bob", "B1");

        var aliceSessions = await repo.GetSessionsByUserAsync("alice");
        var bobSessions = await repo.GetSessionsByUserAsync("bob");

        Assert.Equal(2, aliceSessions.Count);
        Assert.Single(bobSessions);
    }

    [Fact]
    public async Task DeleteSessionAsync_Returns_False_If_Not_Exists()
    {
        using var ctx = CreateContext();
        var repo = new SessionRepository(ctx, NullLogger<SessionRepository>.Instance);

        var result = await repo.DeleteSessionAsync(Guid.NewGuid());
        Assert.False(result);
    }
}
