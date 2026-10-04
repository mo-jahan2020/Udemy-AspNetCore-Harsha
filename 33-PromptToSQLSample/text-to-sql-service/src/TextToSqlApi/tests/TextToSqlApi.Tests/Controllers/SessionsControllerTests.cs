using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TextToSqlApi.Controllers;
using TextToSqlApi.Models.DTOs;
using TextToSqlApi.Services;
using Xunit;

namespace TextToSqlApi.Tests.Controllers;

/// <summary>تست‌های SessionsController.</summary>
public class SessionsControllerTests
{
    [Fact]
    public async Task Create_Returns_CreatedAtAction()
    {
        var sessionMock = new Mock<ISessionService>();
        sessionMock.Setup(s => s.CreateSessionAsync(It.IsAny<CreateSessionRequest>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new SessionDto { Id = Guid.NewGuid(), UserName = "alice" });

        var ctrl = new SessionsController(sessionMock.Object, NullLogger<SessionsController>.Instance);

        var result = await ctrl.Create(new CreateSessionRequest { UserName = "alice" }, default);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var dto = Assert.IsType<SessionDto>(created.Value);
        Assert.Equal("alice", dto.UserName);
    }

    [Fact]
    public async Task GetHistory_Returns_NotFound_When_Missing()
    {
        var sessionMock = new Mock<ISessionService>();
        sessionMock.Setup(s => s.GetSessionHistoryAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync((SessionHistoryDto?)null);

        var ctrl = new SessionsController(sessionMock.Object, NullLogger<SessionsController>.Instance);

        var result = await ctrl.GetHistory(Guid.NewGuid(), default);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task Delete_Returns_NoContent_When_Deleted()
    {
        var sessionMock = new Mock<ISessionService>();
        sessionMock.Setup(s => s.DeleteSessionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(true);

        var ctrl = new SessionsController(sessionMock.Object, NullLogger<SessionsController>.Instance);

        var result = await ctrl.Delete(Guid.NewGuid(), default);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_Returns_NotFound_When_Missing()
    {
        var sessionMock = new Mock<ISessionService>();
        sessionMock.Setup(s => s.DeleteSessionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(false);

        var ctrl = new SessionsController(sessionMock.Object, NullLogger<SessionsController>.Instance);

        var result = await ctrl.Delete(Guid.NewGuid(), default);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task ListByUser_Returns_BadRequest_When_UserName_Empty()
    {
        var sessionMock = new Mock<ISessionService>();
        var ctrl = new SessionsController(sessionMock.Object, NullLogger<SessionsController>.Instance);

        var result = await ctrl.ListByUser("", default);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }
}
