using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TextToSqlApi.Models.DTOs;
using TextToSqlApi.Models.Entities;
using TextToSqlApi.Repositories;
using TextToSqlApi.Services;
using Xunit;

namespace TextToSqlApi.Tests.Services;

/// <summary>تست‌های SessionService با Mock کردن Repository.</summary>
public class SessionServiceTests
{
    [Fact]
    public async Task CreateSessionAsync_Returns_Dto()
    {
        var repoMock = new Mock<ISessionRepository>();
        repoMock.Setup(r => r.CreateSessionAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string userName, string? title, CancellationToken _) => new Session
                {
                    Id = Guid.NewGuid(),
                    UserName = userName,
                    Title = title,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });

        var svc = new SessionService(repoMock.Object, NullLogger<SessionService>.Instance);

        var req = new CreateSessionRequest { UserName = "alice", Title = "Test" };
        var result = await svc.CreateSessionAsync(req);

        Assert.Equal("alice", result.UserName);
        Assert.Equal("Test", result.Title);
    }

    [Fact]
    public async Task AddUserMessageAsync_Throws_When_Session_NotFound()
    {
        var repoMock = new Mock<ISessionRepository>();
        repoMock.Setup(r => r.ExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

        var svc = new SessionService(repoMock.Object, NullLogger<SessionService>.Instance);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            svc.AddUserMessageAsync(Guid.NewGuid(), "prompt"));
    }

    [Fact]
    public async Task GetSessionHistoryAsync_Returns_Null_If_NotFound()
    {
        var repoMock = new Mock<ISessionRepository>();
        repoMock.Setup(r => r.GetSessionAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Session?)null);

        var svc = new SessionService(repoMock.Object, NullLogger<SessionService>.Instance);

        var result = await svc.GetSessionHistoryAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteSessionAsync_Returns_False_If_NotFound()
    {
        var repoMock = new Mock<ISessionRepository>();
        repoMock.Setup(r => r.DeleteSessionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

        var svc = new SessionService(repoMock.Object, NullLogger<SessionService>.Instance);

        var result = await svc.DeleteSessionAsync(Guid.NewGuid());

        Assert.False(result);
    }
}
