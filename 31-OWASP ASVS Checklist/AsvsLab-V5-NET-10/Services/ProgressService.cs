using System.Text.Json;

namespace AsvsLab.Services;

// ═══════════════════════════════════════════════════════════════════
//  پیشرفت مطالعه کاربر (امکان جدید نسخه ۲)
//  دموهای دیده‌شده و نمره کوییز هر فصل در «سشن» کاربر ذخیره می‌شود؛
//  داشبورد درصد پیشرفت هر فصل را نمایش می‌دهد.
//  نکته آموزشی: سشن سمت سرور نگهداری می‌شود (بند V7.2.1 — سشن باید
//  در بک‌اند مورد اعتماد بررسی شود، نه سمت کلاینت)
// ═══════════════════════════════════════════════════════════════════
public class ProgressService
{
    private const string DemoKey = "prog.demos";
    private const string QuizKey = "prog.quiz";
    private readonly IHttpContextAccessor _http;

    public ProgressService(IHttpContextAccessor http) => _http = http;

    private ISession? Session => _http.HttpContext?.Session;

    public void Mark(string demoId)
    {
        var s = Session; if (s is null) return;
        var set = Visited();
        set.Add(demoId);
        s.SetString(DemoKey, JsonSerializer.Serialize(set));
    }

    public HashSet<string> Visited()
    {
        var raw = Session?.GetString(DemoKey);
        return raw is null ? new HashSet<string>() : (JsonSerializer.Deserialize<HashSet<string>>(raw) ?? new());
    }

    public void SetQuizScore(int chapter, int percent)
    {
        var s = Session; if (s is null) return;
        var map = QuizScores();
        if (!map.TryGetValue(chapter, out var old) || percent > old)
            map[chapter] = percent;
        s.SetString(QuizKey, JsonSerializer.Serialize(map));
    }

    public Dictionary<int, int> QuizScores()
    {
        var raw = Session?.GetString(QuizKey);
        if (raw is null) return new Dictionary<int, int>();
        try { return JsonSerializer.Deserialize<Dictionary<int, int>>(raw) ?? new(); }
        catch { return new Dictionary<int, int>(); }
    }

    public void Reset()
    {
        var s = Session; if (s is null) return;
        s.Remove(DemoKey);
        s.Remove(QuizKey);
    }
}
