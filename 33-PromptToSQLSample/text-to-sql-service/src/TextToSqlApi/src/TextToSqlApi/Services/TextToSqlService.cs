using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TextToSqlApi.Models.DTOs;
using TextToSqlApi.Models.Options;

namespace TextToSqlApi.Services;

/// <summary>
/// سرویس هماهنگ‌کننده (Orchestrator) تولید SQL.
/// این سرویس ابتدا به Vanna مراجعه می‌کند؛ اگر در دسترس نبود و Fallback فعال بود،
/// مستقیماً از Ollama استفاده می‌کند. تاریخچه در Session ذخیره می‌شود.
/// </summary>
public interface ITextToSqlService
{
    /// <summary>ثبت/به‌روزرسانی شما دیتابیس (به Vanna ارسال می‌شود).</summary>
    Task UpdateSchemaAsync(UpdateSchemaRequest req, CancellationToken ct = default);

    /// <summary>تولید SQL از پرامپت کاربر و ذخیره در تاریخچه نشست.</summary>
    Task<GenerateSqlResponse> GenerateAsync(GenerateSqlRequest req, CancellationToken ct = default);
}

/// <summary>
/// پیاده‌سازی ITextToSqlService با الگوی Fallback.
/// </summary>
public class TextToSqlService : ITextToSqlService
{
    private readonly IVannaService _vanna;
    private readonly IOllamaService _ollama;
    private readonly ISessionService _sessionService;
    private readonly VannaOptions _vannaOpts;
    private readonly ILogger<TextToSqlService> _logger;

    // شما دیتابیس به‌صورت in-memory در سرویس نگه‌داری می‌شود (برای Fallback به Ollama).
    // فرض منطقی: در یک محیط Production باید در یک cache توزیع‌شده (Redis) ذخیره شود.
    private static string _currentSchema = string.Empty;
    private static readonly object _schemaLock = new();

    /// <summary>سازنده با تزریق وابستگی‌ها.</summary>
    public TextToSqlService(
        IVannaService vanna,
        IOllamaService ollama,
        ISessionService sessionService,
        IOptions<VannaOptions> vannaOpts,
        ILogger<TextToSqlService> logger)
    {
        _vanna = vanna;
        _ollama = ollama;
        _sessionService = sessionService;
        _vannaOpts = vannaOpts.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task UpdateSchemaAsync(UpdateSchemaRequest req, CancellationToken ct = default)
    {
        // ذخیره محلی برای استفاده در Fallback
        lock (_schemaLock)
        {
            _currentSchema = req.SchemaText;
        }

        // ارسال به Vanna برای آموزش
        var vannaHealthy = await _vanna.IsHealthyAsync(ct);
        if (vannaHealthy)
        {
            await _vanna.TrainAsync(req.SchemaText, ct);
            _logger.LogInformation("Schema updated in Vanna (len={Len})", req.SchemaText.Length);
        }
        else if (_vannaOpts.EnableFallbackToOllama)
        {
            _logger.LogWarning("Vanna not available; schema stored locally for Ollama fallback only.");
        }
        else
        {
            throw new InvalidOperationException("Vanna service is not available and fallback is disabled.");
        }
    }

    /// <inheritdoc/>
    public async Task<GenerateSqlResponse> GenerateAsync(GenerateSqlRequest req, CancellationToken ct = default)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        string sql;
        string source;

        // ذخیره پیام کاربر در تاریخچه
        await _sessionService.AddUserMessageAsync(req.SessionId, req.Prompt, ct);

        // تلاش با Vanna
        var vannaHealthy = await _vanna.IsHealthyAsync(ct);
        if (vannaHealthy)
        {
            try
            {
                (sql, _) = await _vanna.GenerateSqlAsync(req.Prompt, ct);
                source = "Vanna";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Vanna generate failed; trying fallback.");
                if (!_vannaOpts.EnableFallbackToOllama)
                    throw;
                (sql, _) = await GenerateViaOllamaAsync(req.Prompt, ct);
                source = "Ollama-Fallback";
            }
        }
        else if (_vannaOpts.EnableFallbackToOllama)
        {
            _logger.LogInformation("Vanna unavailable; using Ollama fallback.");
            (sql, _) = await GenerateViaOllamaAsync(req.Prompt, ct);
            source = "Ollama-Fallback";
        }
        else
        {
            throw new InvalidOperationException("Vanna service is not available and fallback is disabled.");
        }

        sw.Stop();

        // ذخیره پاسخ دستیار در تاریخچه
        await _sessionService.AddAssistantMessageAsync(
            req.SessionId,
            sql,
            sql,
            source,
            sw.ElapsedMilliseconds,
            ct);

        return new GenerateSqlResponse
        {
            Sql = sql,
            SessionId = req.SessionId,
            ResponseTimeMs = sw.ElapsedMilliseconds,
            ModelName = source,
            Source = source
        };
    }

    /// <summary>تولید SQL با Ollama (Fallback) با تزریق شما در System Prompt.</summary>
    private async Task<(string sql, long elapsedMs)> GenerateViaOllamaAsync(string prompt, CancellationToken ct)
    {
        string schema;
        lock (_schemaLock)
        {
            schema = string.IsNullOrEmpty(_currentSchema)
                ? "/* No schema provided. Use POST /api/schema to register the database schema. */"
                : _currentSchema;
        }
        return await _ollama.GenerateSqlAsync(prompt, schema, ct);
    }

    /// <summary>دریافت شما فعلی (برای تست).</summary>
    internal static string GetCurrentSchemaForTest()
    {
        lock (_schemaLock) return _currentSchema;
    }
}
