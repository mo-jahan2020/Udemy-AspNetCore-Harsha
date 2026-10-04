using System.ComponentModel;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.SemanticKernel;

namespace OllamaSK.Api.Plugins;

/// <summary>
/// Plugin جستجوی وب (بدون نیاز به API Key) با چند ارائه‌دهنده:
///   - Parsijoo (پارسیجو)   → موتور جستجوی ایرانی (parsijoo.ir)
///   - Wikipedia (ویکی‌پدیا) → API رسمی فارسی — پایدارترین گزینه
///   - DuckDuckGo           → نسخه HTML (در بعضی شبکه‌ها فیلتر است)
/// ارائه‌دهنده از بخش «Search:Provider» در appsettings انتخاب می‌شود؛
/// «Auto» به ترتیب Parsijoo → Wikipedia → DuckDuckGo تلاش می‌کند و اولین نتیجه را برمی‌گرداند.
/// </summary>
public sealed class SearchPlugin(HttpClient httpClient, IConfiguration configuration, ILogger<SearchPlugin> logger)
{
    // مهلت کوتاه برای هر ارائه‌دهنده تا حالت Auto سریع به گزینه بعدی برود
    private static readonly TimeSpan ProviderTimeout = TimeSpan.FromSeconds(10);

    [KernelFunction("WebSearch")]
    [Description("Searches the web (supports Iranian search engine Parsijoo, Persian Wikipedia and DuckDuckGo) for up-to-date information about a topic and returns top results with titles, links and snippets.")]
    public async Task<string> WebSearch(
        [Description("The search query, e.g. 'latest news about .NET 10'")] string query,
        [Description("Optional provider: Parsijoo | Wikipedia | DuckDuckGo | Auto")] string? provider = null,
        CancellationToken ct = default)
    {
        var chosen = string.IsNullOrWhiteSpace(provider)
            ? configuration["Search:Provider"] ?? "Auto"
            : provider.Trim();

        var max = configuration.GetValue("Search:MaxResults", 5);

        try
        {
            var result = chosen.Trim().ToLowerInvariant() switch
            {
                "parsijoo"   => await SearchParsijooAsync(query, max, ct) ?? "موتور پارسیجو نتیجه‌ای برنگرداند.",
                "wikipedia"  => await SearchWikipediaAsync(query, max, ct) ?? "در ویکی‌پدیای فارسی نتیجه‌ای پیدا نشد.",
                "duckduckgo" => await SearchDuckDuckGoAsync(query, max, ct) ?? "نتیجه‌ای برای این جستجو پیدا نشد.",
                _            => await SearchAutoAsync(query, max, ct)
            };

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "خطا در جستجوی وب برای «{Query}» (ارائه‌دهنده: {Provider})", query, chosen);
            return "جستجوی وب با خطا مواجه شد: " + ex.Message;
        }
    }

    // ─────────────────────────── حالت خودکار ───────────────────────────

    /// <summary>Auto: پارسیجو (ایرانی) ← ویکی‌پدیا فارسی ← DuckDuckGo؛ اولین نتیجه موفق برمی‌گردد</summary>
    private async Task<string> SearchAutoAsync(string query, int max, CancellationToken ct)
    {
        var notes = new List<string>();

        (string Name, Func<CancellationToken, Task<string?>> Runner)[] providers =
        {
            ("پارسیجو", t => SearchParsijooAsync(query, max, t)),
            ("ویکی‌پدیا", t => SearchWikipediaAsync(query, max, t)),
            ("DuckDuckGo", t => SearchDuckDuckGoAsync(query, max, t))
        };

        foreach (var (name, runner) in providers)
        {
            try
            {
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
                timeout.CancelAfter(ProviderTimeout);

                var result = await runner(timeout.Token);
                if (!string.IsNullOrWhiteSpace(result))
                    return result!;

                notes.Add($"{name}: نتیجه‌ای نداشت");
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                notes.Add($"{name}: پاسخ نداد (timeout)");
            }
            catch (Exception ex)
            {
                notes.Add($"{name}: {ex.Message}");
            }
        }

        return "هیچ‌کدام از موتورهای جستجو نتیجه‌ای برنگرداندند:\n- " + string.Join("\n- ", notes);
    }

    // ─────────────────────────── پارسیجو (ایرانی) ───────────────────────────

    /// <summary>
    /// جستجو در پارسیجو parsijoo.ir/web?q=... با پارس HTML مقاوم (چند الگو).
    /// اگر ساختار صفحه تغییر کند یا در دسترس نباشد، null برمی‌گردد تا Auto سراغ گزینه بعد برود.
    /// </summary>
    public async Task<string?> SearchParsijooAsync(string query, int max, CancellationToken ct)
    {
        var html = await DownloadAsync("https://parsijoo.ir/web?q=" + Uri.EscapeDataString(query), ct);
        if (string.IsNullOrEmpty(html))
            return null;

        // الگوهای احتمالی نتیجه در HTML پارسیجو (به‌ترتیب امتحان می‌شوند)
        var patterns = new[]
        {
            // الگو ۱: عنوان داخل h3 با لینک + توضیح با کلاس result-description
            @"<h3[^>]*>\s*<a[^>]*href=""([^""]+)""[^>]*>(.*?)</a>",
            // الگو ۲: لینک با کلاس result__link یا result-link
            @"<a[^>]*class=""(?:result__link|result-link|web-result-title)""[^>]*href=""([^""]+)""[^>]*>(.*?)</a>"
        };

        MatchCollection? titles = null;

        foreach (var pattern in patterns)
        {
            titles = Regex.Matches(html, pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
            if (titles.Count > 0)
                break;
        }

        if (titles is null || titles.Count == 0)
            return null;

        // توضیح نتایج (در صورت وجود)
        var snippets = Regex.Matches(
            html,
            @"class=""(?:result-description|result__snippet|web-result-description)""[^>]*>(.*?)</(?:p|div|span)>",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);

        var sb = new StringBuilder("نتایج پارسیجو برای «" + query + "»:\n");
        var count = Math.Min(max, titles.Count);

        for (var i = 0; i < count; i++)
        {
            var title = StripHtml(titles[i].Groups[2].Value);
            var url = titles[i].Groups[1].Value;
            var snippet = i < snippets.Count ? StripHtml(snippets[i].Groups[1].Value) : string.Empty;

            sb.AppendLine($"{i + 1}. {title}");
            sb.AppendLine($"   {url}");
            if (snippet.Length > 0)
                sb.AppendLine($"   {snippet}");
        }

        return sb.ToString();
    }

    // ─────────────────────────── ویکی‌پدیا فارسی ───────────────────────────

    /// <summary>
    /// جستجو در ویکی‌پدیای فارسی با API رسمی (JSON) — پایدارترین گزینه، به‌خصوص در ایران.
    /// </summary>
    public async Task<string?> SearchWikipediaAsync(string query, int max, CancellationToken ct)
    {
        var url = "https://fa.wikipedia.org/w/api.php?action=query&format=json&list=search" +
                  $"&srlimit={Math.Clamp(max, 1, 10)}&srsearch=" + Uri.EscapeDataString(query);

        var json = await DownloadAsync(url, ct);
        if (string.IsNullOrEmpty(json))
            return null;

        using var doc = JsonDocument.Parse(json);
        var search = doc.RootElement.GetProperty("query").GetProperty("search");

        if (search.GetArrayLength() == 0)
            return null;

        var sb = new StringBuilder("نتایج ویکی‌پدیای فارسی برای «" + query + "»:\n");
        var i = 0;

        foreach (var item in search.EnumerateArray())
        {
            var title = item.GetProperty("title").GetString() ?? string.Empty;
            var snippet = StripHtml(item.TryGetProperty("snippet", out var s) ? s.GetString() ?? string.Empty : string.Empty);
            var link = "https://fa.wikipedia.org/wiki/" + Uri.EscapeDataString(title.Replace(' ', '_'));

            sb.AppendLine($"{++i}. {title}");
            sb.AppendLine($"   {link}");
            if (snippet.Length > 0)
                sb.AppendLine($"   {snippet}");
        }

        return sb.ToString();
    }

    // ─────────────────────────── DuckDuckGo ───────────────────────────

    /// <summary>جستجوی DuckDuckGo با نسخه HTML (در صورت فیلتر بودن، null برمی‌گردد)</summary>
    public async Task<string?> SearchDuckDuckGoAsync(string query, int max, CancellationToken ct)
    {
        var html = await DownloadAsync("https://html.duckduckgo.com/html/?q=" + Uri.EscapeDataString(query), ct);
        if (string.IsNullOrEmpty(html))
            return null;

        var titles = Regex.Matches(
            html,
            "<a[^>]*class=\"result__a\"[^>]*href=\"([^\"]+)\"[^>]*>(.*?)</a>",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);

        if (titles.Count == 0)
            return null;

        var snippets = Regex.Matches(
            html,
            "class=\"result__snippet\"[^>]*>(.*?)</a>",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);

        var sb = new StringBuilder("نتایج DuckDuckGo برای «" + query + "»:\n");
        var count = Math.Min(max, titles.Count);

        for (var i = 0; i < count; i++)
        {
            var title = StripHtml(titles[i].Groups[2].Value);
            var url = CleanDuckDuckGoUrl(titles[i].Groups[1].Value);
            var snippet = i < snippets.Count ? StripHtml(snippets[i].Groups[1].Value) : string.Empty;

            sb.AppendLine($"{i + 1}. {title}");
            sb.AppendLine($"   {url}");
            if (snippet.Length > 0)
                sb.AppendLine($"   {snippet}");
        }

        return sb.ToString();
    }

    // ─────────────────────────── ابزارهای مشترک ───────────────────────────

    /// <summary>دانلود متن پاسخ — ویکی‌پدیا UA اختصاصی با اطلاعات تماس می‌خواهد (سیاست ربات‌های Wikimedia)</summary>
    private async Task<string?> DownloadAsync(string url, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.UserAgent.ParseAdd("OllamaSK-Demo/1.0 (educational sample; contact: dev@example.com)");

        using var response = await httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("دانلود از {Url} ناموفق بود: {Code}", url, (int)response.StatusCode);
            return null;
        }

        return await response.Content.ReadAsStringAsync(ct);
    }

    /// <summary>حذف تگ‌های HTML و رمزگشایی کاراکترهای خاص</summary>
    private static string StripHtml(string html)
        => WebUtility.HtmlDecode(Regex.Replace(html, "<[^>]+>", string.Empty)).Trim();

    /// <summary>لینک‌های DuckDuckGo ریدایرکت هستند؛ آدرس واقعی از پارامتر uddg استخراج می‌شود</summary>
    private static string CleanDuckDuckGoUrl(string rawUrl)
    {
        var index = rawUrl.IndexOf("uddg=", StringComparison.Ordinal);
        if (index >= 0)
        {
            var url = rawUrl[(index + 5)..];
            var amp = url.IndexOf('&');
            if (amp >= 0)
                url = url[..amp];
            return Uri.UnescapeDataString(url);
        }

        return rawUrl.StartsWith("//") ? "https:" + rawUrl : rawUrl;
    }
}
