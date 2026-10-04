using AsvsLab.Services;
using Microsoft.AspNetCore.Mvc;
// نام مستعار: چون اکشنِ همین کنترلر «PasswordPolicy» نام دارد،
// برای رسیدن به کلاس سیاست گذرواژه از نام مستعار استفاده می‌کنیم
using PolicySvc = AsvsLab.Services.PasswordPolicy;

namespace AsvsLab.Controllers;

// ═══════════════════════════════════════════════════════════════════
//  فصل V6 — احراز هویت (Authentication)
//  شامل سیاست گذرواژه، قفل‌گذاری (Redis و درون‌حافظه) و هش امن
//  جریان واقعی ورود/ثبت‌نام در کنترلر Account پیاده‌سازی شده است
// ═══════════════════════════════════════════════════════════════════
public class V6Controller : DemoBaseController
{
    private readonly RedisLockoutService _redis;
    private readonly InMemoryLockoutService _memory;

    public V6Controller(AsvsRefService asvs, ProgressService progress,
        RedisLockoutService redis, InMemoryLockoutService memory) : base(asvs, progress)
    {
        _redis = redis;
        _memory = memory;
    }

    public IActionResult Index() =>
        SectionIndex(6, "احراز هویت: سیاست گذرواژه (V6.2)، قفل‌گذاری ضد Brute Force (V6.3) و هش امن گذرواژه. ورود واقعی برنامه با کاربران admin/Admin123! و ali/Ali12345! در صفحه «ورود» آزمایش کنید.");

    // ─────────────────────────────────────────────────────────────
    // دموی ۱: هش امن گذرواژه — بند V11.4.2 (در 5.0 به فصل رمزنگاری منتقل شد)
    // ─────────────────────────────────────────────────────────────
    public IActionResult PasswordHash()
    {
        var r = New("v6-hash", "V11.4.2",
            "گذرواژه‌ها با KDF تأییدشده و «مصرف‌کننده منابع» ذخیره شوند. در 5.0 این بند در فصل V11 آمده (معادل V2.4.1 در 4.0.3).", "V6");
        r.ServiceNote = "بدون وابستگی خارجی — PBKDF2 از System.Security.Cryptography";
        r.VulnerableCode = """
            // ❌ ناامن: MD5/SHA1 خام یا بدتر از همه: ذخیره متن آشکار
            var hash = MD5.HashData(Encoding.UTF8.GetBytes(password));
            // MD5 در میلی‌ثانیه شکسته می‌شود؛ GPU سالی میلیاردها هش می‌زند
            """;
        r.SecureCode = """
            // ✅ امن: PBKDF2 با نمک تصادفی و ۱۰۰ هزار تکرار
            var salt = RandomNumberGenerator.GetBytes(20);
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt,
                100_000, HashAlgorithmName.SHA256, 32);
            // بررسی با CryptographicOperations.FixedTimeEquals (ضد Timing Attack)
            """;

        var hasher = HttpContext.RequestServices.GetRequiredService<PasswordHasher>();
        var pw = "Ali12345!";
        var h1 = hasher.Hash(pw);
        var h2 = hasher.Hash(pw); // همان گذرواژه، هش متفاوت (نمک متفاوت)

        r.OutputLines.Add($"گذرواژه یکسان «{pw}» دو بار هش شد:");
        r.OutputLines.Add($"هش اول: {h1}");
        r.OutputLines.Add($"هش دوم: {h2}");
        r.OutputLines.Add($"دو هش متفاوت‌اند؟ {(h1 != h2 ? "بله ✅ (نمک تصادفی)" : "خیر ❌")}");
        r.OutputLines.Add($"بررسی با گذرواژه درست:  {hasher.Verify(pw, h1)}");
        r.OutputLines.Add($"بررسی با گذرواژه غلط:    {hasher.Verify("WrongPass", h1)}");
        r.OutputLines.Add("توضیح: نمک تصادفی باعث می‌شود Rainbow Table بی‌اثر شود؛ تکرار بالا سرعت مهاجم را می‌کُشد.");
        return View("../Shared/DemoResult", r);
    }

    // ─────────────────────────────────────────────────────────────
    // دموی ۲: سیاست گذرواژه — بندهای V6.2.1/2.5/2.9/2.11/2.12 (فرم)
    // ─────────────────────────────────────────────────────────────
    [HttpGet] public IActionResult PasswordPolicy() => FormView("v6-policy", "q", "گذرواژه برای بررسی سیاست", "مثال: Ali12345! یا 123456");

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult PasswordPolicy(string q)
    {
        var r = New("v6-policy", "V6.2.1,V6.2.5,V6.2.9,V6.2.11,V6.2.12",
            "سیاست گذرواژه در ASVS 5.0 مبتنی بر NIST 800-63B است: حداقل طول، بدون قواعد ترکیب، پذیرش ۶۴ کاراکتر و بررسی فهرست لو رفته.", "V6");
        var pw = string.IsNullOrWhiteSpace(q) ? "Ali12345!" : q;
        var checks = PolicySvc.Evaluate(pw);
        foreach (var c in checks)
            r.OutputLines.Add($"{(c.Passed ? "✅" : "🚫")} [{c.Ref}] {c.FaTitle}: {c.Detail}");
        var passed = checks.Count(c => c.Passed);
        r.OutputLines.Add($"نتیجه: {passed} از {checks.Count} معیار رعایت شد.");
        r.OutputLines.Add("نکته 5.0: دیگر «الزام علائم خاص» نداریم — طول + فهرست لو رفته مهم‌تر است (بند V6.2.5).");
        return View("../Shared/DemoResult", r);
    }

    // ─────────────────────────────────────────────────────────────
    // دموی ۳: بررسی گذرواژه لو رفته — بند V6.2.4 (فرم)
    // ─────────────────────────────────────────────────────────────
    [HttpGet] public IActionResult BreachCheck() => FormView("v6-breach", "q", "گذرواژه برای بررسی لو رفته", "مثال: qwerty");

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult BreachCheck(string q)
    {
        var r = New("v6-breach", "V6.2.4",
            "گذرواژه‌ها هنگام ثبت‌نام/تغییر باید در برابر فهرست گذرواژه‌های لو رفته بررسی شوند.", "V6");
        r.VulnerableCode = """
            // ❌ ناامن: پذیرش هر گذرواژه‌ای حتی «123456»
            """;
        r.SecureCode = """
            // ✅ امن: مقایسه با فهرست لو رفته (در واقعیت: API دارند گزارش شده مثل HaveIBeenPwned)
            if (BreachedList.Contains(password)) return BadRequest("این گذرواژه لو رفته است");
            """;
        var pw = string.IsNullOrWhiteSpace(q) ? "qwerty" : q;
        var hit = PolicySvc.BreachedList.Contains(pw);
        r.OutputLines.Add($"گذرواژه: «{pw}»");
        r.OutputLines.Add(hit
            ? "🚫 در فهرست لو رفته یافت شد! (یکی از رایج‌ترین گذرواژه‌های جهان)"
            : "✅ در فهرست دمو یافت نشد (فهرست کامل در HaveIBeenPwned با k-Anonymity بررسی می‌شود)");
        r.OutputLines.Add($"فهرست دمو: {PolicySvc.BreachedList.Count} گذرواژه رایج جهان");
        return View("../Shared/DemoResult", r);
    }

    // ─────────────────────────────────────────────────────────────
    // دموی ۴: قفل‌گذاری توزیع‌شده با Redis — بند V6.3.1
    // ⚠️ پیش‌نیاز: سرویس Redis روی localhost:6379
    // ─────────────────────────────────────────────────────────────
    public IActionResult RedisLockout()
    {
        var r = New("v6-redis", "V6.3.1",
            "قفل‌گذاری توزیع‌شده با INCR اتمی + پنجره زمانی EXPIRE؛ در چند سرور هم درست کار می‌کند.", "V6");
        r.ServiceNote = "⚠️ این دمو برای اجرای درست به سرویس Redis روی localhost:6379 نیاز دارد. " +
                        "نصب: Windows (wsl: sudo apt install redis-server) | Ubuntu: sudo apt install redis-server | Docker: docker run -p 6379:6379 redis";

        if (!_redis.IsAvailable)
        {
            r.Success = false;
            r.OutputLines.Add("❌ اتصال به Redis برقرار نشد (localhost:6379).");
            r.OutputLines.Add("این بخش عمداً با کاهش‌یافته طراحی شده تا برنامه کرش نکند.");
            r.OutputLines.Add("راهکار جایگزین بدون نصب: دموی «قفل‌گذاری درون‌حافظه‌ای» همین فصل را ببینید.");
            r.OutputLines.Add("دستور نصب سریع: docker run -d -p 6379:6379 redis");
            return View("../Shared/DemoResult", r);
        }

        var user = "attacker-demo";
        for (var i = 1; i <= 6; i++)
        {
            var (locked, count, msg) = _redis.RegisterFailedAttempt(user);
            r.OutputLines.Add($"تلاش {i}: {(locked ? "🚫 " : "🔑 ")}{msg}");
            if (locked) break;
        }
        r.OutputLines.Add(_redis.ResetAttempts(user));
        return View("../Shared/DemoResult", r);
    }

    // ─────────────────────────────────────────────────────────────
    // دموی ۵: قفل‌گذاری درون‌حافظه‌ای — بند V6.3.1 (بدون وابستگی)
    // ─────────────────────────────────────────────────────────────
    public IActionResult MemoryLockout()
    {
        var r = New("v6-memory", "V6.3.1",
            "نسخه درون‌حافظه‌ای قفل‌گذاری: ۵ تلاش ناموفق در پنجره ۱۰ دقیقه‌ای → قفل.", "V6");
        r.ServiceNote = "بدون وابستگی خارجی؛ برای چند instance سرور از نسخه Redis استفاده کنید.";
        var svc = HttpContext.RequestServices.GetRequiredService<InMemoryLockoutService>();
        var key = "demo-user|" + HttpContext.Connection.RemoteIpAddress;
        svc.Reset(key);
        for (var i = 1; i <= 6; i++)
        {
            svc.RegisterFailure(key);
            var locked = svc.IsLockedOut(key, out var remaining);
            r.OutputLines.Add($"تلاش {i}: {(locked ? "🚫 حساب قفل شد!" : $"✅ مجاز — {remaining} تلاش باقی مانده")}");
        }
        r.OutputLines.Add("نکته: صفحه ورود واقعی برنامه (Account) همین منطق را روی هر کاربر اعمال می‌کند.");
        svc.Reset(key);
        return View("../Shared/DemoResult", r);
    }

    // ─────────────────────────────────────────────────────────────
    // دموی متنی: جریان واقعی احراز هویت — بندهای V6.2.2/V6.2.3
    // ─────────────────────────────────────────────────────────────
    public IActionResult RealLogin() =>
        View("../Shared/DemoResult", TextDemo("v6-login", "V6.2.2,V6.2.3",
            "کاربر باید بتواند وارد شود و گذرواژه خود را تغییر دهد؛ تغییر گذرواژه نیازمند گذرواژه فعلی است.", "V6",
            "این برنامه یک سیستم احراز هویت واقعی دارد (نه شبیه‌سازی):\n\n" +
            "• صفحه «ورود» در منو → با کاربران دمو: admin / Admin123! (نقش Admin) یا ali / Ali12345! (نقش User)\n" +
            "• ثبت‌نام واقعی با اعمال سیاست V6.2 (فقط گذرواژه‌های غیرلو رفته پذیرفته می‌شود)\n" +
            "• قفل‌گذاری ۵ تلاش ناموفق (V6.3.1) روی فرم ورود فعال است — امتحان کنید!\n" +
            "• تغییر گذرواژه با درخواست «گذرواژه فعلی + جدید» طبق بند V6.2.3 در پروفایل کاربران قابل توسعه است.\n\n" +
            "کوکی احراز هویت با HttpOnly + SameSite=Lax و عمر ۳۰ دقیقه صادر می‌شود و پس از ورود، سشن قبلی پاک می‌شود (V7.2.4)."));
}
