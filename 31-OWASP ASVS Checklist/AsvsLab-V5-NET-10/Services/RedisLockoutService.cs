using StackExchange.Redis;

namespace AsvsLab.Services;

// ═══════════════════════════════════════════════════════════════════
//  بند V6.3.1 — قفل‌گذاری توزیع‌شده با Redis
//
//  ⚠️ پیش‌نیاز سرویس: این بخش برای اجرای درست نیاز به سرویس Redis
//  روی localhost:6379 دارد. اگر Redis نصب نباشد، برنامه کرش نمی‌کند؛
//  پیام راهنمای نصب (فارسی) نمایش داده می‌شود و می‌توانید از نسخه
//  درون‌حافظه‌ای (InMemoryLockoutService) استفاده کنید.
//
//  الگوی پیاده‌سازی: دستور اتمی INCR برای شمارش تلاش‌های ناموفق +
//  EXPIRE برای پنجره زمانی — در چند instance کار می‌کند.
// ═══════════════════════════════════════════════════════════════════
public class RedisLockoutService
{
    private const string Config = "localhost:6379,abortConnect=false";
    private const int MaxAttempts = 5;
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(10);

    private static readonly Lazy<ConnectionMultiplexer?> _redis = new(() =>
    {
        try
        {
            // تایم‌اوت کوتاه تا در نبود Redis، برنامه سریع و با ملایمت ادامه دهد
            var opts = new ConfigurationOptions
            {
                EndPoints = { "localhost:6379" },
                AbortOnConnectFail = false,
                ConnectTimeout = 500,   // نیم ثانیه
                ConnectRetry = 0,
                SyncTimeout = 1000,
            };
            var mux = ConnectionMultiplexer.Connect(opts);
            return mux.IsConnected ? mux : null;
        }
        catch { return null; }   // Redis در دسترس نیست → حالت کاهش‌یافته
    });

    public bool IsAvailable => _redis.Value is not null && _redis.Value.IsConnected;

    /// <summary>ثبت تلاش ناموفق؛ خروجی: (قفل شد؟ تعداد تلاش‌ها، پیام فارسی)</summary>
    public (bool Locked, int Count, string Message) RegisterFailedAttempt(string username)
    {
        var db = _redis.Value?.GetDatabase();
        if (db is null)
            return (false, 0, "⚠️ سرویس Redis در دسترس نیست — بند V6.3.1 نیاز به قفل‌گذاری توزیع‌شده دارد.");

        var key = $"asvslab:lockout:{username.ToLowerInvariant()}";
        try
        {
            var count = (int)db.StringIncrement(key);          // INCR — عملیات اتمی
            if (count == 1) db.KeyExpire(key, Window);          // EXPIRE — پنجره ۱۰ دقیقه‌ای
            if (count >= MaxAttempts)
                return (true, count, $"🚫 حساب قفل شد! {count} تلاش ناموفق در پنجره ۱۰ دقیقه‌ای.");
            return (false, count, $"تلاش ناموفق {count} از {MaxAttempts} — پس از {MaxAttempts} تلاش حساب قفل می‌شود.");
        }
        catch (RedisException ex)
        {
            return (false, 0, $"خطای Redis: {ex.Message}");
        }
    }

    /// <summary>بازنشانی تلاش‌ها (مثلاً پس از ورود موفق)</summary>
    public string ResetAttempts(string username)
    {
        var db = _redis.Value?.GetDatabase();
        if (db is null) return "Redis در دسترس نیست؛ بازنشانی انجام نشد.";
        db.KeyDelete($"asvslab:lockout:{username.ToLowerInvariant()}");
        return $"شمارنده تلاش‌های ناموفق «{username}» در Redis بازنشانی شد.";
    }
}
