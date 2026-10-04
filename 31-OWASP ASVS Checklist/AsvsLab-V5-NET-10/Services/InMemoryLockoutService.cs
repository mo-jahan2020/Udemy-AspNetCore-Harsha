using System.Collections.Concurrent;

namespace AsvsLab.Services;

// ═══════════════════════════════════════════════════════════════════
//  بند V6.3.1 — کنترل‌های ضد حملات Brute Force و Credential Stuffing
//  نسخه درون‌حافظه‌ای: بدون هیچ وابستگی خارجی؛ مناسب تک‌سرور
//  (نسخه توزیع‌شده با Redis در RedisLockoutService پیاده‌سازی شده است)
// ═══════════════════════════════════════════════════════════════════
public class InMemoryLockoutService
{
    private readonly ConcurrentDictionary<string, (int Count, DateTime FirstTry)> _attempts = new();
    private const int MaxAttempts = 5;              // حداکثر ۵ تلاش ناموفق
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(10); // در پنجره ۱۰ دقیقه‌ای

    public bool IsLockedOut(string key, out int remaining)
    {
        remaining = MaxAttempts;
        if (!_attempts.TryGetValue(key, out var a)) return false;
        if (DateTime.UtcNow - a.FirstTry > Window) { _attempts.TryRemove(key, out _); return false; }
        remaining = Math.Max(0, MaxAttempts - a.Count);
        return a.Count >= MaxAttempts;
    }

    /// <summary>ثبت تلاش ناموفق؛ خروجی: آیا حساب قفل شد؟</summary>
    public bool RegisterFailure(string key)
    {
        _attempts.AddOrUpdate(key,
            _ => (1, DateTime.UtcNow),
            (_, old) => DateTime.UtcNow - old.FirstTry > Window ? (1, DateTime.UtcNow) : (old.Count + 1, old.FirstTry));
        return _attempts.TryGetValue(key, out var a) && a.Count >= MaxAttempts;
    }

    public void Reset(string key) => _attempts.TryRemove(key, out _);

    public int TotalTracked => _attempts.Count;
}
