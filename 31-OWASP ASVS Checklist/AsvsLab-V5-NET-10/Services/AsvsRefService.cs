using System.Text.Json;

namespace AsvsLab.Services;

// ═══════════════════════════════════════════════════════════════════
//  بند V15.1.2 — کاتالوگ مراجع: متن رسمی ۳۴۵ بند ASVS 5.0
//  این سرویس فایل Data/asvs5.json را (که از مخزن رسمی OWASP/ASVS
//  تگ v5.0.0_release استخراج شده) بارگذاری می‌کند تا هر دمو بتواند
//  «متن دقیق انگلیسی بند» را کنار توضیح فارسی نمایش دهد.
// ═══════════════════════════════════════════════════════════════════
public class AsvsRefService
{
    private readonly Dictionary<string, Req> _reqs = new();
    private readonly Dictionary<string, JsonElement> _chapters = new();

    public record Req(string Id, string En, string Section, int Level);

    public AsvsRefService(IWebHostEnvironment env)
    {
        var path = Path.Combine(env.ContentRootPath, "Data", "asvs5.json");
        if (!File.Exists(path))
            path = Path.Combine(AppContext.BaseDirectory, "Data", "asvs5.json");

        if (File.Exists(path))
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            foreach (var p in doc.RootElement.GetProperty("chapters").EnumerateObject())
                _chapters[p.Name] = p.Value.Clone();
            foreach (var p in doc.RootElement.GetProperty("requirements").EnumerateObject())
            {
                var v = p.Value;
                _reqs[p.Name] = new Req(p.Name,
                    v.GetProperty("en").GetString() ?? "",
                    v.GetProperty("section").GetString() ?? "",
                    v.GetProperty("level").GetInt32());
            }
        }
    }

    /// <summary>متن رسمی یک یا چند بند (با کاما جدا شوند) برای نمایش در کارت دمو</summary>
    public string Official(string refsCsv)
    {
        var parts = refsCsv.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var lines = parts.Select(r =>
        {
            var req = _reqs.GetValueOrDefault(r);
            return req is null ? $"{r}: (متن بند در دسترس نیست)" : $"{r} [سطح {req.Level}]: {req.En}";
        });
        return string.Join("\n\n", lines);
    }

    public Req? Get(string id) => _reqs.GetValueOrDefault(id);

    /// <summary>عنوان فارسی و انگلیسی فصل</summary>
    public (string Fa, string En) Chapter(int ch)
    {
        if (_chapters.TryGetValue(ch.ToString(), out var c))
            return (c.GetProperty("fa").GetString() ?? "", c.GetProperty("en").GetString() ?? "");
        return ("", "");
    }

    /// <summary>تعداد بندهای رسمی هر فصل — برای نمایش در داشبورد</summary>
    public int CountOf(int ch) =>
        _chapters.TryGetValue(ch.ToString(), out var c) ? c.GetProperty("count").GetInt32() : 0;

    public int Total => _reqs.Count;
}
