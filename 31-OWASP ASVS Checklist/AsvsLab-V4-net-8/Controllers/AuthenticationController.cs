using System.Collections.Concurrent;
using AsvsLab.Models;
using AsvsLab.Services;
using Microsoft.AspNetCore.Mvc;

namespace AsvsLab.Controllers;

// ============================================================================
// 🔐 بخش اول: احراز هویت (Authentication)
// فصل V2 استاندارد OWASP ASVS 4.0.3
// هر Action این کنترلر یک دکمه در صفحه اصلی است و یکی از بندهای استاندارد
// را به‌صورت عملی اجرا و توضیح می‌دهد.
// ============================================================================
public class AuthenticationController : Controller
{
    private readonly PasswordHasher _hasher;
    private readonly DemoUserStore _userStore;
    private readonly RedisLockoutService _redisLockout;

    // شمارنده تلاش‌های ناموفق درون‌حافظه‌ای (فقط برای این دموی محلی)
    private static readonly ConcurrentDictionary<string, int> InMemoryAttempts = new();

    // ------------------------------------------------------------------------
    // فهرست دموهای این بخش (صفحه اول کنترلر)
    // ------------------------------------------------------------------------
    private static readonly SectionCatalog Catalog = new()
    {
        Title = "بخش ۱: احراز هویت (Authentication)",
        ChapterRef = "فصل V2 — OWASP ASVS 4.0.3",
        Demos =
        {
            new() { Action = "PasswordHash", Title = "هش امن رمز عبور با PBKDF2 + Salt", AsvsRef = "V2.4.1 — V2.4.3" },
            new() { Action = "PasswordPolicy", Title = "بررسی سیاست رمز عبور و رمزهای لو رفته (فرم ورودی)", AsvsRef = "V2.1.1 — V2.1.9" },
            new() { Action = "RedisLockout", Title = "قفل‌سازی حساب توزیع‌شده با Redis ⚠️نیازمند سرویس Redis", AsvsRef = "V2.2.1" },
            new() { Action = "InMemoryLockout", Title = "قفل‌سازی حساب با شمارنده درون‌حافظه‌ای", AsvsRef = "V2.2.1" },
        }
    };

    [HttpGet]
    public IActionResult Index() => View("../Shared/SectionIndex", Catalog);

    public AuthenticationController(PasswordHasher hasher, DemoUserStore userStore, RedisLockoutService redisLockout)
    {
        _hasher = hasher;
        _userStore = userStore;
        _redisLockout = redisLockout;
    }

    // ------------------------------------------------------------------------
    // 🎯 دمو ۱: هش امن رمز عبور با PBKDF2
    //
    // ⚙️ بندهای استاندارد OWASP ASVS 4.0.3:
    //   V2.4.1 → رمز عبور باید Salt شده و با تابع مشتق‌سازی کلید تأییدشده هش شود
    //   V2.4.2 → Salt حداقل ۳۲ بیت و برای هر کاربر منحصربه‌فرد
    //   V2.4.3 → در PBKDF2 تعداد تکرار حداقل ۱۰۰,۰۰۰ بار
    // ------------------------------------------------------------------------
    [HttpGet]
    public IActionResult PasswordHash()
    {
        var result = new DemoResult
        {
            Title = "هش امن رمز عبور با PBKDF2 + Salt اختصاصی",
            AsvsRef = "V2.4.1 — V2.4.2 — V2.4.3",
            AsvsSummary = "رمز عبور باید با تابع مشتق‌سازی کلید (KDF) تأییدشده، همراه Salt منحصربه‌فرد حداقل ۳۲ بیتی و تعداد تکرار مناسب (حداقل ۱۰۰ هزار تکرار برای PBKDF2) ذخیره شود.",
            Explanation = "هرگز رمز عبور را خام (Plain Text) یا با هش ساده مانند MD5 ذخیره نکنید؛ چون مهاجمی که دیتابیس را می‌دزدد با جداول رنگین‌کمان (Rainbow Table) آن‌ها را فوراً بازیابی می‌کند. در این دمو رمز عبور «Admin@Strong2024» با PBKDF2-HMAC-SHA256 و ۱۰۰ هزار تکرار هش می‌شود. به خروجی دقت کنید: هشِ یک رمز ثابت، در دو بار اجرا متفاوت است؛ چون هر بار Salt تصادفی جدیدی تولید می‌شود. دقیقاً به همین دلیل است که دو کاربر با رمز یکسان، هش متفاوتی در دیتابیس دارند و مهاجم نمی‌فهمد رمزشان یکی است.",
            CodeSnippet = """
                // تولید Salt تصادفی ۱۲۸ بیتی (بند V2.4.2)
                var salt = RandomNumberGenerator.GetBytes(16);

                // PBKDF2 با SHA-256 و ۱۰۰,۰۰۰ تکرار (بندهای V2.4.1 و V2.4.3)
                using var pbkdf2 = new Rfc2898DeriveBytes(
                    password, salt, iterations: 100_000, HashAlgorithmName.SHA256);
                var hash = pbkdf2.GetBytes(32);

                // مقایسه امن در زمان ثابت هنگام تأیید رمز
                CryptographicOperations.FixedTimeEquals(hash, storedHash);
                """,
            BackController = "Authentication"
        };

        // ------------------------- اجرای واقعی دمو -------------------------
        const string password = "Admin@Strong2024";

        var (hash1, salt1) = _hasher.Hash(password);
        var (hash2, salt2) = _hasher.Hash(password); // همان رمز، هش دوم!

        result.OutputLines.Add($"رمز عبور ورودی: {password}");
        result.OutputLines.Add("");
        result.OutputLines.Add($"[بار اول] Salt تولیدشده: {salt1}");
        result.OutputLines.Add($"[بار اول] هش نهایی (Base64): {hash1}");
        result.OutputLines.Add("");
        result.OutputLines.Add($"[بار دوم] Salt تولیدشده: {salt2}");
        result.OutputLines.Add($"[بار دوم] هش نهایی (Base64): {hash2}");
        result.OutputLines.Add("");
        result.OutputLines.Add(salt1 != salt2
            ? "✓ Saltها متفاوت‌اند → بنابراین هش دو بارِ یک رمز یکسان هم متفاوت است (ضد Rainbow Table)"
            : "✗ خطا: Salt تکراری شد!");
        result.OutputLines.Add("");

        // تأیید رمز درست و رمز غلط
        bool okCorrect = _hasher.Verify(password, hash1, salt1);
        bool okWrong = _hasher.Verify("WrongPassword123", hash1, salt1);
        result.OutputLines.Add($"تأیید با رمز درست  → {okCorrect}  (باید True باشد)");
        result.OutputLines.Add($"تأیید با رمز غلط   → {okWrong}   (باید False باشد)");

        return View("../Shared/DemoResult", result);
    }

    // ------------------------------------------------------------------------
    // 🎯 دمو ۲ (فرم ورودی): سیاست رمز عبور استاندارد + فهرست رمزهای لو رفته
    //
    // ⚙️ بندهای استاندارد OWASP ASVS 4.0.3:
    //   V2.1.1 → حداقل طول ۱۲ کاراکتر
    //   V2.1.2 → مجاز بودن رمز بلند (تا ۶۴) و رد شدن بیش از ۱۲۸ کاراکتر
    //   V2.1.7 → بررسی رمز در برابر فهرست رمزهای لو رفته
    //   V2.1.8 → ارائه سنجش‌گر قدرت رمز عبور
    //   V2.1.9 → ممنوعیت قواعد اجباری ترکیب کاراکتر
    // ------------------------------------------------------------------------
    [HttpGet]
    public IActionResult PasswordPolicy(string? input)
    {
        const string asvsRef = "V2.1.1 — V2.1.2 — V2.1.7 — V2.1.8 — V2.1.9";
        const string summary = "حداقل طول رمز ۱۲ کاراکتر، مجاز بودن رمزهای بلند، بررسی در برابر فهرست رمزهای لو رفته، نمایش سنجش‌گر قدرت و نداشتن قواعد اجباری ترکیب کاراکتر.";

        // اگر ورودی نیامده، فرم نمایش داده می‌شود
        if (string.IsNullOrEmpty(input))
        {
            return View("../Shared/DemoForm", new DemoFormViewModel
            {
                Title = "بررسی سیاست رمز عبور استاندارد",
                AsvsRef = asvsRef,
                AsvsSummary = summary,
                Description = "یک رمز عبور وارد کنید تا بر اساس بندهای V2.1 استاندارد بررسی شود. برای مشاهده تفاوت، یک بار «123456» و یک بار رمز بلند و متفاوت امتحان کنید. حتی می‌توانید از فاصله و ایموجی هم استفاده کنید (بند V2.1.4 کاراکترهای چاپی یونیکد را مجاز می‌داند).",
                InputLabel = "رمز عبور آزمایشی",
                FormAction = "/Authentication/PasswordPolicy",
                IsPassword = true
            });
        }

        var result = new DemoResult
        {
            Title = "بررسی سیاست رمز عبور استاندارد",
            AsvsRef = asvsRef,
            AsvsSummary = summary,
            Explanation = "استاندارد جدید (مبتنی بر NIST 800-63) به‌جای اجبار «حرف بزرگ + عدد + نماد»، روی «طول» و «نبودن در فهرست رمزهای لو رفته» تمرکز دارد؛ چون قواعد ترکیب اجباری، کاربران را به الگوهای قابل حدس مثل Password1! سوق می‌دهد. رمز واردشده شما با قواعد واقعیِ برنامه بررسی شد.",
            CodeSnippet = """
                // V2.1.1: حداقل ۱۲ کاراکتر (پس از ادغام فاصله‌های متوالی)
                if (normalized.Length < 12) reject("خیلی کوتاه");

                // V2.1.7: بررسی در برابر فهرست رمزهای لو رفته
                if (BreachedPasswords.Contains(password))
                    reject("رمز عبور لو رفته است");

                // V2.1.9: هیچ اجباری برای حرف بزرگ/عدد/نماد وجود ندارد
                """,
            BackController = "Authentication"
        };

        result.OutputLines.Add($"رمز واردشده: {(input.Length <= 40 ? input : input[..40] + "...")}");
        result.OutputLines.Add(new string('-', 60));
        foreach (var check in AsvsLab.Services.PasswordPolicy.Evaluate(input))
        {
            result.OutputLines.Add($"{(check.Passed ? "✓" : "✗")} {check.Rule}");
            result.OutputLines.Add($"   └─ {check.Note}");
        }

        result.Success = AsvsLab.Services.PasswordPolicy.Evaluate(input).All(c => c.Passed);
        return View("../Shared/DemoResult", result);
    }

    // ------------------------------------------------------------------------
    // 🎯 دمو ۳: قفل‌سازی حساب در برابر حمله Brute Force با Redis
    //
    // ⚙️ بند استاندارد OWASP ASVS 4.0.3:
    //   V2.2.1 → کنترل‌های ضد-اتوماسیون باید جلوی حملات Brute Force و آزمودن
    //            رمزهای لو رفته را بگیرند (Rate Limiting / Soft Lockout)
    //
    // ⚠️⚠️ پیش‌نیاز اجرای این دمو:
    //   سرویس Redis باید روی رایانه شما نصب و اجرا شده باشد (localhost:6379)
    //   اگر Redis در دسترس نباشد، پیام راهنما نمایش داده می‌شود.
    // ------------------------------------------------------------------------
    [HttpGet]
    public IActionResult RedisLockout()
    {
        var result = new DemoResult
        {
            Title = "قفل‌سازی حساب توزیع‌شده با Redis (ضد Brute Force)",
            AsvsRef = "V2.2.1",
            AsvsSummary = "کنترل‌های ضد-اتوماسیون مانند محدودسازی نرخ و قفل‌سازی نرم باید جلوی تلاش‌های مکرر ورود را بگیرند (حداکثر ۱۰۰ تلاش ناموفق در ساعت برای یک حساب).",
            Explanation = "شمارنده تلاش‌های ناموفق را با دستور اتمیک INCR در Redis نگه می‌داریم؛ بنابراین اگر اپلیکیشن را روی چند سرور اجرا کنید، همه یک شمارنده مشترک خواهند داشت (این کاری است که شمارنده درون‌حافظه‌ای از پس آن برنمی‌آید). پس از ۵ تلاش ناموفق، حساب برای ۱۰ دقیقه قفل می‌شود (Soft Lockout) و پیام عمومی نشان داده می‌شود تا مهاجم نفهمد رمز درست بود یا نام کاربری وجود نداشت. این دمو ۶ تلاش ناموفق برای کاربر «victim_user» ثبت می‌کند.",
            CodeSnippet = """
                // شمارش اتمیک تلاش‌های ناموفق در Redis
                string key = $"lockout:{username}";
                var attempts = (int)db.StringIncrement(key);
                if (attempts == 1)
                    db.KeyExpire(key, TimeSpan.FromMinutes(10)); // پنجره ۱۰ دقیقه‌ای

                bool isLocked = attempts > 5; // قفل نرم پس از ۵ تلاش
                """,
            ServiceRequirement = "برای اجرای این دمو سرویس Redis باید روی رایانه شما نصب و اجرا باشد (localhost:6379).",
            BackController = "Authentication"
        };

        var (attempts, isLocked, error) = _redisLockout.RegisterFailedAttempt("victim_user");

        if (error is not null)
        {
            // گرِیس‌فول: اگر Redis نبود، برنامه کرش نمی‌کند
            result.Success = false;
            result.OutputLines.Add("✗ اتصال به Redis برقرار نشد!");
            result.OutputLines.Add("");
            result.OutputLines.Add("برای اجرای این دمو باید سرویس Redis نصب و اجرا شده باشد:");
            result.OutputLines.Add("  • ویندوز (پیشنهادی): نصب Memurai یا Redis از طریق WSL");
            result.OutputLines.Add("  • داخل WSL:  sudo apt install redis-server && sudo service redis-server start");
            result.OutputLines.Add("  • لینوکس:    sudo apt install redis-server && sudo systemctl start redis");
            result.OutputLines.Add("");
            result.OutputLines.Add("پس از اجرا شدن سرویس، دوباره روی این دکمه بزنید.");
        }
        else
        {
            // برای شبیه‌سازی حمله، چند بار دیگر تلاش ناموفق ثبت می‌کنیم
            var (a2, l2, _) = _redisLockout.RegisterFailedAttempt("victim_user");
            var (a3, l3, _) = _redisLockout.RegisterFailedAttempt("victim_user");
            var (a4, l4, _) = _redisLockout.RegisterFailedAttempt("victim_user");
            var (a5, l5, _) = _redisLockout.RegisterFailedAttempt("victim_user");
            var (a6, l6, _) = _redisLockout.RegisterFailedAttempt("victim_user");

            result.OutputLines.Add("شبیه‌سازی ۶ تلاش ناموفق ورود برای کاربر victim_user:");
            result.OutputLines.Add($"  تلاش ۱ → شمارنده: {attempts} | قفل: {(isLocked ? "بله" : "خیر")}");
            result.OutputLines.Add($"  تلاش ۲ → شمارنده: {a2} | قفل: {(l2 ? "بله" : "خیر")}");
            result.OutputLines.Add($"  تلاش ۳ → شمارنده: {a3} | قفل: {(l3 ? "بله" : "خیر")}");
            result.OutputLines.Add($"  تلاش ۴ → شمارنده: {a4} | قفل: {(l4 ? "بله" : "خیر")}");
            result.OutputLines.Add($"  تلاش ۵ → شمارنده: {a5} | قفل: {(l5 ? "بله" : "خیر")}");
            result.OutputLines.Add($"  تلاش ۶ → شمارنده: {a6} | قفل: {(l6 ? "بله ⛔ حساب قفل شد" : "خیر")}");
            result.OutputLines.Add("");
            result.OutputLines.Add("✓ شمارنده در Redis با کلید lockout:victim_user ذخیره شده است.");
            result.OutputLines.Add("  (می‌توانید در redis-cli با دستور GET lockout:victim_user آن را ببینید)");
            result.OutputLines.Add("");
            result.OutputLines.Add("نکته: پیام خطا به مهاجم همیشه عمومی است: «نام کاربری یا رمز عبور نادرست است»");
        }

        return View("../Shared/DemoResult", result);
    }

    // ------------------------------------------------------------------------
    // 🎯 دمو ۴: قفل‌سازی حساب با شمارنده درون‌حافظه‌ای (بدون نیاز به سرویس جانبی)
    //
    // ⚙️ بند استاندارد OWASP ASVS 4.0.3: V2.2.1
    // ------------------------------------------------------------------------
    [HttpGet]
    public IActionResult InMemoryLockout()
    {
        var result = new DemoResult
        {
            Title = "قفل‌سازی حساب با شمارنده درون‌حافظه‌ای",
            AsvsRef = "V2.2.1",
            AsvsSummary = "محدودسازی نرخ تلاش ورود (Rate Limiting) و قفل‌سازی نرم حساب پس از تلاش‌های ناموفق متوالی.",
            Explanation = "این همان منطق دموی قبلی است اما بدون سرویس جانبی: شمارنده در حافظه پروسه (ConcurrentDictionary) نگه‌داری می‌شود. برای اپلیکیشن تک‌سروره کافی است؛ اما اگر چند نمونه از برنامه پشت Load Balancer اجرا شود، هر نمونه شمارنده خودش را دارد و مهاجم می‌تواند با پخش تلاش‌ها بین سرورها از قفل فرار کند. به همین دلیل در محیط واقعی، راهکار توزیع‌شده (مانند دموی Redis) توصیه می‌شود.",
            CodeSnippet = """
                // شمارنده درون‌حافظه‌ای (تک-سرور)
                var attempts = InMemoryAttempts.AddOrUpdate(
                    username, 1, (_, c) => c + 1);

                if (attempts > 5)
                    return Error("حساب موقتاً قفل شده است");
                """,
            BackController = "Authentication"
        };

        result.OutputLines.Add("شبیه‌سازی ۷ تلاش ناموفق برای کاربر admin_demo:");
        for (int i = 1; i <= 7; i++)
        {
            var attempts = InMemoryAttempts.AddOrUpdate("admin_demo", 1, (_, c) => c + 1);
            result.OutputLines.Add($"  تلاش {i} → شمارنده: {attempts} | {(attempts > 5 ? "⛔ قفل شد (Soft Lockout)" : "اجازه تلاش مجدد")}");
        }
        result.OutputLines.Add("");
        result.OutputLines.Add("در پروژه واقعی، پس از انقضای پنجره زمانی (مثلاً ۱۰ دقیقه) یا ورود موفق، شمارنده ریست می‌شود.");

        return View("../Shared/DemoResult", result);
    }
}
