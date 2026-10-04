using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TextToSqlApi.Models.DTOs;
using TextToSqlApi.Models.Options;
using TextToSqlApi.Services;
using Xunit;

namespace TextToSqlApi.Tests.Services;

/// <summary>تست‌های TextToSqlService با Fallback از Vanna به Ollama.</summary>
public class TextToSqlServiceTests
{
    private static VannaOptions MakeVannaOpts(bool fallback = true) => new()
    {
        EnableFallbackToOllama = fallback,
        BaseUrl = "http://localhost:8001",
        TimeoutSeconds = 30
    };

    [Fact]
    public async Task GenerateAsync_Uses_Ollama_Fallback_When_Vanna_Unavailable()
    {
        var vannaMock = new Mock<IVannaService>();
        vannaMock.Setup(v => v.IsHealthyAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);
        vannaMock.Setup(v => v.GenerateSqlAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ThrowsAsync(new InvalidOperationException("Vanna down"));

        var ollamaMock = new Mock<IOllamaService>();
        ollamaMock.Setup(o => o.GenerateSqlAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(("SELECT 1", 50L));

        var sessionMock = new Mock<ISessionService>();
        sessionMock.Setup(s => s.AddUserMessageAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new SessionMessageDto());
        sessionMock.Setup(s => s.AddAssistantMessageAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<long?>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new SessionMessageDto());

        var svc = new TextToSqlService(
            vannaMock.Object, ollamaMock.Object, sessionMock.Object,
            Microsoft.Extensions.Options.Options.Create(MakeVannaOpts(true)),
            NullLogger<TextToSqlService>.Instance);

        var result = await svc.GenerateAsync(new GenerateSqlRequest
        {
            SessionId = Guid.NewGuid(),
            Prompt = "show all customers"
        });

        Assert.Equal("SELECT 1", result.Sql);
        Assert.Equal("Ollama-Fallback", result.Source);
        ollamaMock.Verify(o => o.GenerateSqlAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GenerateAsync_Throws_When_Vanna_Unavailable_And_Fallback_Disabled()
    {
        var vannaMock = new Mock<IVannaService>();
        vannaMock.Setup(v => v.IsHealthyAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var ollamaMock = new Mock<IOllamaService>();
        var sessionMock = new Mock<ISessionService>();
        sessionMock.Setup(s => s.AddUserMessageAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new SessionMessageDto());

        var svc = new TextToSqlService(
            vannaMock.Object, ollamaMock.Object, sessionMock.Object,
            Microsoft.Extensions.Options.Options.Create(MakeVannaOpts(false)),
            NullLogger<TextToSqlService>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.GenerateAsync(new GenerateSqlRequest { SessionId = Guid.NewGuid(), Prompt = "test" }));
    }

    [Fact]
    public async Task GenerateAsync_Uses_Vanna_When_Healthy()
    {
        var vannaMock = new Mock<IVannaService>();
        vannaMock.Setup(v => v.IsHealthyAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        vannaMock.Setup(v => v.GenerateSqlAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(("SELECT 1", 30L));

        var ollamaMock = new Mock<IOllamaService>();
        var sessionMock = new Mock<ISessionService>();
        sessionMock.Setup(s => s.AddUserMessageAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new SessionMessageDto());
        sessionMock.Setup(s => s.AddAssistantMessageAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<long?>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new SessionMessageDto());

        var svc = new TextToSqlService(
            vannaMock.Object, ollamaMock.Object, sessionMock.Object,
            Microsoft.Extensions.Options.Options.Create(MakeVannaOpts(true)),
            NullLogger<TextToSqlService>.Instance);

        var result = await svc.GenerateAsync(new GenerateSqlRequest
        {
            SessionId = Guid.NewGuid(),
            Prompt = "show customers"
        });

        Assert.Equal("SELECT 1", result.Sql);
        Assert.Equal("Vanna", result.Source);
        ollamaMock.Verify(o => o.GenerateSqlAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateSchemaAsync_Falls_Back_When_Vanna_Unavailable()
    {
        var vannaMock = new Mock<IVannaService>();
        vannaMock.Setup(v => v.IsHealthyAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var ollamaMock = new Mock<IOllamaService>();
        var sessionMock = new Mock<ISessionService>();

        var svc = new TextToSqlService(
            vannaMock.Object, ollamaMock.Object, sessionMock.Object,
            Microsoft.Extensions.Options.Options.Create(MakeVannaOpts(true)),
            NullLogger<TextToSqlService>.Instance);

        var req = new UpdateSchemaRequest { SchemaText = "CREATE TABLE Customers (Id INT)" };
        var ex = await Record.ExceptionAsync(() => svc.UpdateSchemaAsync(req));

        Assert.Null(ex);
        // شما باید محلی ذخیره شده باشد
        Assert.Equal("CREATE TABLE Customers (Id INT)", TextToSqlService.GetCurrentSchemaForTest());
    }
}
