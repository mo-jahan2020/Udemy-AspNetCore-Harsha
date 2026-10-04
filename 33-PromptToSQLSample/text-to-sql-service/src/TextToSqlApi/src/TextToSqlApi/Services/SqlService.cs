using System.Data;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TextToSqlApi.Models.DTOs;

namespace TextToSqlApi.Services;

/// <summary>
/// پیاده‌سازی ISqlService با Microsoft.Data.SqlClient.
/// این سرویس به دیتابیس هدف کاربر متصل می‌شود و فقط Query های Read-Only را اجرا می‌کند.
/// </summary>
public partial class SqlService : ISqlService
{
    private readonly string _connectionString;
    private readonly ILogger<SqlService> _logger;

    /// <summary>سازنده با خواندن Connection String از IConfiguration.</summary>
    public SqlService(IConfiguration config, ILogger<SqlService> logger)
    {
        _connectionString = config.GetConnectionString("TargetDatabase")
            ?? throw new InvalidOperationException("ConnectionStrings:TargetDatabase is not configured.");
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<bool> IsHealthyAsync(CancellationToken ct = default)
    {
        try
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);
            return conn.State == ConnectionState.Open;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Target database health-check failed");
            return false;
        }
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">اگر SQL حاوی دستورات مخرب باشد.</exception>
    public async Task<ExecuteSqlResponse> ExecuteReadOnlyAsync(string sql, int maxRows = 100, CancellationToken ct = default)
    {
        ValidateReadOnly(sql);

        var sw = Stopwatch.StartNew();
        var response = new ExecuteSqlResponse();

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.CommandType = CommandType.Text;
        cmd.CommandTimeout = 30;

        await using var reader = await cmd.ExecuteReaderAsync(ct);

        // خواندن اسامی ستون‌ها
        for (var i = 0; i < reader.FieldCount; i++)
            response.Columns.Add(reader.GetName(i));

        // خواندن ردیف‌ها با رعایت MaxRows
        while (await reader.ReadAsync(ct) && response.Rows.Count < maxRows)
        {
            var row = new Dictionary<string, object?>();
            for (var i = 0; i < reader.FieldCount; i++)
            {
                var colName = response.Columns[i];
                row[colName] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            }
            response.Rows.Add(row);
        }

        response.RowCount = response.Rows.Count;
        sw.Stop();
        response.ExecutionTimeMs = sw.ElapsedMilliseconds;

        _logger.LogInformation("Executed read-only SQL: {Rows} rows in {Ms}ms", response.RowCount, sw.ElapsedMilliseconds);
        return response;
    }

    /// <summary>
    /// اعتبارسنجی Read-Only بودن SQL با Regex و لیست سیاه.
    /// دستورات زیر مسدود می‌شوند: DROP, DELETE, UPDATE, TRUNCATE, ALTER, INSERT, MERGE, GRANT, REVOKE, EXEC, xp_cmdshell.
    /// </summary>
    internal static void ValidateReadOnly(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
            throw new InvalidOperationException("SQL is empty.");

        // Normalization: حذف کامنت‌ها و فاصله‌های اضافی
        var normalized = SqlCommentRegex().Replace(sql, " ");
        normalized = WhitespaceRegex().Replace(normalized, " ").Trim().ToUpperInvariant();

        if (string.IsNullOrEmpty(normalized))
            throw new InvalidOperationException("SQL is empty after removing comments.");

        // باید با SELECT شروع شود (اجازه WITH/CTE هم هست)
        if (!normalized.StartsWith("SELECT") && !normalized.StartsWith("WITH") && !normalized.StartsWith(";WITH"))
            throw new InvalidOperationException("Only SELECT queries are allowed (must start with SELECT or WITH).");

        // لیست سیاه دستورات مخرب
        var blacklist = new[]
        {
            "DROP", "DELETE", "UPDATE", "TRUNCATE", "ALTER", "INSERT",
            "MERGE", "GRANT", "REVOKE", "EXEC", "EXECUTE", "XP_CMDSHELL",
            "SHUTDOWN", "BACKUP", "RESTORE", "CREATE", "ATTACH", "DETACH"
        };

        foreach (var kw in blacklist)
        {
            // تطبیق کلمه‌ای کامل (با کلمه‌باندری)
            var pattern = $@"\b{kw}\b";
            if (Regex.IsMatch(normalized, pattern, RegexOptions.IgnoreCase))
                throw new InvalidOperationException($"Blocked: keyword '{kw}' is not allowed in read-only mode.");
        }

        // جلوگیری از Semicolon-Chaining که دستور دوم را مخفی کند
        if (normalized.Contains(";"))
        {
            // فقط اجازه یک Semicolon پایانی بده
            var trimmed = normalized.TrimEnd(';', ' ');
            if (trimmed.Contains(";"))
                throw new InvalidOperationException("Multiple statements are not allowed (semicolons).");
        }
    }

    /// <summary>Regex برای حذف کامنت‌های SQL (-- و /* */).</summary>
    [GeneratedRegex(@"(--[^\n\r]*|/\*[\s\S]*?\*/)", RegexOptions.Compiled)]
    private static partial Regex SqlCommentRegex();

    /// <summary>Regex برای فشرده‌سازی فضاهای خالی.</summary>
    [GeneratedRegex(@"\s+", RegexOptions.Compiled)]
    private static partial Regex WhitespaceRegex();
}
