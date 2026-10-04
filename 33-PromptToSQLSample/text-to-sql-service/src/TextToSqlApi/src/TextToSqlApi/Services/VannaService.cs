using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TextToSqlApi.Models.Options;

namespace TextToSqlApi.Services;

/// <summary>
/// پیاده‌سازی IVannaService که به میکروسرویس Python (FastAPI + Vanna) متصل می‌شود.
/// اگر سرویس Python در دسترس نباشد، باید توسط سرویس بالادستی از Ollama به‌صورت Fallback استفاده شود.
/// </summary>
public class VannaService : IVannaService
{
    private readonly HttpClient _http;
    private readonly VannaOptions _opts;
    private readonly ILogger<VannaService> _logger;

    /// <summary>سازنده با تزریق HttpClient و تنظیمات Vanna.</summary>
    public VannaService(HttpClient http, IOptions<VannaOptions> opts, ILogger<VannaService> logger)
    {
        _http = http;
        _opts = opts.Value;
        _logger = logger;

        _http.BaseAddress = new Uri(_opts.BaseUrl.TrimEnd('/') + "/");
        _http.Timeout = TimeSpan.FromSeconds(_opts.TimeoutSeconds);
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    /// <inheritdoc/>
    public async Task<bool> IsHealthyAsync(CancellationToken ct = default)
    {
        try
        {
            using var resp = await _http.GetAsync("health", ct);
            return resp.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Vanna health-check failed at {BaseUrl}", _opts.BaseUrl);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task TrainAsync(string schemaText, CancellationToken ct = default)
    {
        var payload = new { schema = schemaText };
        var json = JsonSerializer.Serialize(payload);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var resp = await _http.PostAsync("train", content, ct);
        var body = await resp.Content.ReadAsStringAsync(ct);

        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogError("Vanna train failed: {Status} {Body}", resp.StatusCode, body);
            throw new InvalidOperationException($"Vanna train failed: {resp.StatusCode}. Body: {body}");
        }

        _logger.LogInformation("Vanna schema training updated (len={Len})", schemaText.Length);
    }

    /// <inheritdoc/>
    public async Task<(string sql, long elapsedMs)> GenerateSqlAsync(string prompt, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        var payload = new { prompt };
        var json = JsonSerializer.Serialize(payload);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var resp = await _http.PostAsync("generate", content, ct);
        var body = await resp.Content.ReadAsStringAsync(ct);
        sw.Stop();

        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogError("Vanna generate failed: {Status} {Body}", resp.StatusCode, body);
            throw new InvalidOperationException($"Vanna generate failed: {resp.StatusCode}. Body: {body}");
        }

        using var doc = JsonDocument.Parse(body);
        var sql = doc.RootElement.GetProperty("sql").GetString() ?? string.Empty;
        _logger.LogInformation("Vanna generated SQL in {ElapsedMs}ms (len={Len})", sw.ElapsedMilliseconds, sql.Length);
        return (sql, sw.ElapsedMilliseconds);
    }
}
