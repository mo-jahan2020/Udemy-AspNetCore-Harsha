using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TextToSqlApi.Models.Options;

namespace TextToSqlApi.Services;

/// <summary>
/// پیاده‌سازی سرویس Ollama که با endpoint رسمی /api/generate کار می‌کند.
/// این سرویس به‌عنوان Fallback نیز استفاده می‌شود (وقتی Vanna در دسترس نباشد).
/// </summary>
public class OllamaService : IOllamaService
{
    private readonly HttpClient _http;
    private readonly OllamaOptions _opts;
    private readonly ILogger<OllamaService> _logger;

    /// <summary>سازنده با تزریق HttpClient و تنظیمات Ollama.</summary>
    public OllamaService(HttpClient http, IOptions<OllamaOptions> opts, ILogger<OllamaService> logger)
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
            // یک درخواست سبک به /api/tags برای بررسی سلامت سرویس
            using var resp = await _http.GetAsync("api/tags", ct);
            return resp.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ollama health-check failed at {BaseUrl}", _opts.BaseUrl);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<(string sql, long elapsedMs)> GenerateSqlAsync(string prompt, string schemaText, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        var systemPrompt = BuildSystemPrompt(schemaText);

        var payload = new
        {
            model = _opts.Model,
            prompt = prompt,
            system = systemPrompt,
            stream = false,
            options = new
            {
                temperature = _opts.Temperature,
                num_predict = _opts.MaxTokens
            }
        };

        var json = JsonSerializer.Serialize(payload);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var resp = await _http.PostAsync("api/generate", content, ct);
        var body = await resp.Content.ReadAsStringAsync(ct);
        sw.Stop();

        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogError("Ollama generate failed: {Status} {Body}", resp.StatusCode, body);
            throw new InvalidOperationException($"Ollama generate failed: {resp.StatusCode}. Body: {body}");
        }

        using var doc = JsonDocument.Parse(body);
        var response = doc.RootElement.GetProperty("response").GetString() ?? string.Empty;

        var sql = ExtractSql(response);
        _logger.LogInformation("Ollama generated SQL in {ElapsedMs}ms (len={Len})", sw.ElapsedMilliseconds, sql.Length);
        return (sql, sw.ElapsedMilliseconds);
    }

    /// <summary>
    /// ساخت System Prompt که شمای دیتابیس و قواعد تولید SQL را به مدل تزریق می‌کند.
    /// </summary>
    private string BuildSystemPrompt(string schemaText) =>
        $$"""
        You are an expert SQL Server (T-SQL) query generator.
        Your task: convert the user's natural-language request into a single, valid, read-only T-SQL query.

        STRICT RULES:
        1. Output ONLY the SQL query. No markdown, no explanations, no comments.
        2. Use ONLY SELECT statements. NEVER use INSERT, UPDATE, DELETE, DROP, ALTER, TRUNCATE, MERGE, GRANT, REVOKE, EXEC, or xp_cmdshell.
        3. Use the schema provided below. Use exact table and column names.
        4. Use TOP N (e.g. TOP 100) when the user might expect many rows.
        5. Use square brackets [] around object names if they contain spaces or reserved words.
        6. If the request cannot be answered from the schema, return: SELECT NULL AS Error;
        7. Use proper JOINs; prefer explicit JOIN syntax over implicit joins.
        8. Always alias tables for readability.

        DATABASE SCHEMA:
        ----------------
        {{schemaText}}
        ----------------
        """;

    /// <summary>
    /// استخراج SQL خالص از خروجی مدل (حذف ```sql fences و توضیحات اضافی).
    /// </summary>
    internal static string ExtractSql(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return string.Empty;

        var text = raw.Trim();

        // حذف markdown fences
        if (text.StartsWith("```"))
        {
            var firstNewline = text.IndexOf('\n');
            if (firstNewline >= 0)
                text = text[(firstNewline + 1)..];
            if (text.EndsWith("```"))
                text = text[..^3];
            text = text.Trim();
        }

        // اگر هنوز ``` باقی مانده، آنها را حذف کن
        text = text.Replace("```sql", "").Replace("```", "").Trim();
        return text;
    }
}
