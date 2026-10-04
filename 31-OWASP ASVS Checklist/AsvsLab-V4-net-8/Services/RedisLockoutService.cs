using StackExchange.Redis;

namespace AsvsLab.Services;

// ============================================================================
// ⚙️ بند V2.2.1 استاندارد OWASP ASVS 4.0.3:
// «کنترل‌های ضد-اتوماسیون باید در برابر آزمودن رمزهای لو رفته، حملات Brute
//  Force و قفل‌سازی حساب مؤثر باشند؛ شامل محدودسازی نرخ (Rate Limiting) و
//  قفل‌سازی نرم (Soft Lockout)»
//
// ⚠️⚠️ پیش‌نیاز اجرای این سرویس:
// برای اجرای درست بخش‌های وابسته به این سرویس، سرویس Redis باید روی رایانه
// شما نصب و در حال اجرا باشد (آدرس پیش‌فرض: localhost:6379).
//   - ویندوز: Redis را می‌توان از طریق WSL (دستور: sudo apt install redis-server)
//     یا نسخه‌های Memurai / Redis for Windows نصب کرد.
//   - لینوکس: sudo apt install redis-server  سپس:  sudo systemctl start redis
// اگر Redis در دسترس نباشد، دمو به‌جای کرش کردن، پیام راهنما نشان می‌دهد.
// ============================================================================
public class RedisLockoutService
{
    private const int MaxAttempts = 5;            // حداکثر تلاش ناموفق مجاز
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(10); // پنجره زمانی شمارش

    private readonly Lazy<ConnectionMultiplexer?> _connection;
    private readonly ILogger<RedisLockoutService> _logger;

    public RedisLockoutService(ILogger<RedisLockoutService> logger)
    {
        _logger = logger;
        // اتصال تنبل (Lazy): تا اولین استفاده تلاش برای اتصال نمی‌شود
        _connection = new Lazy<ConnectionMultiplexer?>(() =>
        {
            try
            {
                // ⚠️ آدرس سرویس Redis روی رایانه شما — باید نصب و اجرا باشد
                return ConnectionMultiplexer.Connect("localhost:6379,abortConnect=false,connectTimeout=2000,syncTimeout=2000");
            }
            catch (Exception ex)
            {
                // اگر Redis نصب/اجرا نباشد اینجا با پیام راهنما مواجه می‌شوید
                _logger.LogWarning(ex, "اتصال به Redis برقرار نشد");
                return null;
            }
        });
    }

    /// <summary>
    /// ثبت یک تلاش ناموفق ورود برای کاربر و تعیین اینکه حساب باید قفل شود یا نه.
    /// از دستور INCR اتمیک Redis + EXPIRE استفاده می‌شود تا شمارش بین چند
    /// سرور/نمونه اپلیکیشن هم مشترک و دقیق بماند (مزیت اصلی Redis).
    /// </summary>
    public (int Attempts, bool IsLocked, string? Error) RegisterFailedAttempt(string username)
    {
        try
        {
            var db = TryGetDatabase();
            if (db is null)
                return (0, false, "سرویس Redis در دسترس نیست.");

            string key = $"lockout:{username.ToLowerInvariant()}";

            // INCR اتمیک: افزایش شمارنده تلاش‌های ناموفق
            var attempts = (int)db.StringIncrement(key);

            if (attempts == 1)
                // EXPIRE: پنجره زمانی ۱۰ دقیقه‌ای برای شمارش (تلاش‌ها قدیمی نمی‌مانند)
                db.KeyExpire(key, Window);

            return (attempts, attempts > MaxAttempts, null);
        }
        catch (RedisConnectionException)
        {
            // اگر Redis نصب/اجرا نباشد، برنامه کرش نمی‌کند؛ پیام راهنما نمایش داده می‌شود
            return (0, false, "سرویس Redis در دسترس نیست.");
        }
        catch (RedisTimeoutException)
        {
            return (0, false, "پاسخ سرویس Redis بیش از حد طول کشید.");
        }
    }

    /// <summary>ریست کردن شمارنده پس از ورود موفق (مطابق رویه قفل‌سازی نرم)</summary>
    public bool ResetAttempts(string username)
    {
        try
        {
            var db = TryGetDatabase();
            if (db is null) return false;
            db.KeyDelete($"lockout:{username.ToLowerInvariant()}");
            return true;
        }
        catch (RedisException)
        {
            return false;
        }
    }

    private IDatabase? TryGetDatabase()
    {
        try
        {
            var multiplexer = _connection.Value;
            return multiplexer?.GetDatabase();
        }
        catch (RedisConnectionException)
        {
            return null;
        }
    }
}
