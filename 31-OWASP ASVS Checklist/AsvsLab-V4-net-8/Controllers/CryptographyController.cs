using System.Security.Cryptography;
using System.Text;
using AsvsLab.Models;
using Microsoft.AspNetCore.Mvc;

namespace AsvsLab.Controllers;

// ============================================================================
// 🔑 بخش پنجم: رمزنگاری
// فصل V6 استاندارد OWASP ASVS 4.0.3
// ============================================================================
public class CryptographyController : Controller
{
    // ------------------------------------------------------------------------
    // فهرست دموهای این بخش (صفحه اول کنترلر)
    // ------------------------------------------------------------------------
    private static readonly SectionCatalog Catalog = new()
    {
        Title = "بخش ۵: رمزنگاری",
        ChapterRef = "فصل V6 — OWASP ASVS 4.0.3",
        Demos =
        {
            new() { Action = "AesGcm", Title = "رمزنگاری احرازشده AES-256-GCM + تشخیص دستکاری", AsvsRef = "V6.2.1 — V6.2.7" },
            new() { Action = "WeakCrypto", Title = "چرا MD5 و ECB ممنوع‌اند", AsvsRef = "V6.2.5" },
            new() { Action = "RandomToken", Title = "توکن تصادفی امن (CSPRNG) در برابر Random", AsvsRef = "V6.3.1 — V6.3.2" },
            new() { Action = "Hmac", Title = "امضای پیام با HMAC-SHA256", AsvsRef = "V6.2.7 — V6.2.8" },
        }
    };

    [HttpGet]
    public IActionResult Index() => View("../Shared/SectionIndex", Catalog);

    // ------------------------------------------------------------------------
    // 🎯 دمو ۱: رمزنگاری احرازشده AES-GCM (محرمانگی + یکپارچگی همزمان)
    //
    // ⚙️ بندهای استاندارد OWASP ASVS 4.0.3:
    //   V6.2.2 → استفاده از الگوریتم‌ها و کتابخانه‌های استاندارد و تأییدشده
    //   V6.2.3 → پیکربندی امن IV و حالت بلوکی (Block Mode)
    //   V6.2.6 → Nonce/IV نباید با یک کلید بیشتر از یک بار استفاده شود
    //   V6.2.7 → داده رمزشده باید با امضا، حالت احرازشده یا HMAC محافظت شود
    //   V6.2.1 → ماژول‌های رمزنگاری باید به‌صورت امن شکست بخورند (ضد Padding Oracle)
    // ------------------------------------------------------------------------
    [HttpGet]
    public IActionResult AesGcm()
    {
        var result = new DemoResult
        {
            Title = "رمزنگاری احرازشده AES-256-GCM + تشخیص دستکاری",
            AsvsRef = "V6.2.1 — V6.2.2 — V6.2.3 — V6.2.6 — V6.2.7",
            AsvsSummary = "از الگوریتم‌های استاندارد با IV امن و غیرتکراری استفاده شود و رمزداده با حالت احرازشده (GCM) در برابر دستکاری محافظت شود.",
            Explanation = "AES-GCM علاوه بر محرمانگی، «برچسب احراز» (Authentication Tag) تولید می‌کند؛ اگر حتی یک بیت از رمزدسته تغییر کند، هنگام رمزگشایی خطا می‌دهد و حمله‌های دستکاری/Padding Oracle بی‌اثر می‌شوند (بند V6.2.1 — شکست امن). در این دمو: رمزنگاری یک پیام، سپس دستکاری عمدی یک بایت و مشاهده باز شدن خطا. توجه کنید که Nonce هر بار تصادفی و ۱۲ بایتی است (بند V6.2.6).",
            CodeSnippet = """
                // رمزنگاری با AES-256-GCM (حالت احرازشده - بند V6.2.7)
                byte[] key = RandomNumberGenerator.GetBytes(32);  // کلید ۲۵۶ بیتی
                byte[] nonce = RandomNumberGenerator.GetBytes(12); // Nonce ۹۶ بیتی (V6.2.3 و V6.2.6)

                using var aes = new AesGcm(key, tagSizeInBytes: 16);
                byte[] ciphertext = new byte[plaintext.Length];
                byte[] tag = new byte[16];
                aes.Encrypt(nonce, plaintext, ciphertext, tag);

                // رمزگشایی: اگر رمزدسته دستکاری شده باشد، AuthenticationTagMismatchException
                // پرتاب می‌شود → «شکست امن» (بند V6.2.1)
                """,
            BackController = "Cryptography"
        };

        // ------------------------- اجرای واقعی دمو -------------------------
        string message = "شماره کارت مشتری: 6104-XXXX-XXXX-1234";
        byte[] plaintext = Encoding.UTF8.GetBytes(message);
        byte[] key = RandomNumberGenerator.GetBytes(32);   // کلید ۲۵۶ بیتی
        byte[] nonce = RandomNumberGenerator.GetBytes(12); // Nonce منحصربه‌فرد

        var encryptedBytes = new byte[plaintext.Length];
        var tag = new byte[16];

        // رمزنگاری واقعی
        using (var aes = new AesGcm(key, 16))
        {
            aes.Encrypt(nonce, plaintext, encryptedBytes, tag);
        }

        result.OutputLines.Add($"پیام اصلی: {message}");
        result.OutputLines.Add($"کلید (Base64): {Convert.ToBase64String(key)}");
        result.OutputLines.Add($"Nonce تصادفی (Base64): {Convert.ToBase64String(nonce)}  ← هر بار اجرا متفاوت است (V6.2.6)");
        result.OutputLines.Add($"رمزدسته (Base64): {Convert.ToBase64String(encryptedBytes)}");
        result.OutputLines.Add($"برچسب احراز/Authentication Tag (Base64): {Convert.ToBase64String(tag)}");
        result.OutputLines.Add("");

        // رمزگشایی سالم
        using (var aes = new AesGcm(key, 16))
        {
            var decrypted = new byte[encryptedBytes.Length];
            aes.Decrypt(nonce, encryptedBytes, tag, decrypted);
            result.OutputLines.Add($"【۱】 رمزگشایی با داده سالم: {Encoding.UTF8.GetString(decrypted)} ✓");
        }

        // دستکاری عمدی: تغییر یک بایت از رمزدسته
        byte[] tampered = (byte[])encryptedBytes.Clone();
        tampered[0] ^= 0xFF;

        result.OutputLines.Add("");
        result.OutputLines.Add("【۲】 مهاجم یک بایت از رمزدسته را دستکاری می‌کند (tampered[0] ^= 0xFF):");
        try
        {
            using var aes = new AesGcm(key, 16);
            var hacked = new byte[tampered.Length];
            aes.Decrypt(nonce, tampered, tag, hacked);
            result.OutputLines.Add("   ❌ رمزگشایی موفق شد (اینجا یعنی حمله کار کرد!)");
        }
        catch (CryptographicException ex)
        {
            // بند V6.2.1: شکست امن — نه فقط خطا می‌دهد، بلکه هیچ داده‌ای برگردانده نمی‌شود
            result.OutputLines.Add($"   ⛔ خطای رمزنگاری: {ex.GetType().Name}");
            result.OutputLines.Add("   ✓ حمله دستکاری کشف و رد شد؛ هیچ داده‌ای برگردانده نشد (شکست امن - V6.2.1)");
        }

        return View("../Shared/DemoResult", result);
    }

    // ------------------------------------------------------------------------
    // 🎯 دمو ۲: چرا الگوریتم‌های ضعیف (MD5 / ECB) ممنوع‌اند
    //
    // ⚙️ بند استاندارد OWASP ASVS 4.0.3:
    //   V6.2.5 → الگوریتم‌های ناامن (ECB، Triple-DES، MD5، SHA1 و...) مگر برای
    //            سازگاری با سیستم قدیمی، استفاده نشوند
    // ------------------------------------------------------------------------
    [HttpGet]
    public IActionResult WeakCrypto()
    {
        var result = new DemoResult
        {
            Title = "الگوریتم‌های ضعیف (MD5 و حالت ECB) در برابر الگوریتم‌های استاندارد",
            AsvsRef = "V6.2.5",
            AsvsSummary = "الگوریتم‌های هش و رمزنگاری ناامن (مانند MD5، SHA-1 و حالت ECB) نباید استفاده شوند.",
            Explanation = "MD5 سال‌هاست شکسته است: با GPUهای امروزی، میلیاردها هش در ثانیه قابل محاسبه است و «برخورد» (Collision) هم پیدا می‌شود — یعنی دو داده متفاوت با یک هش! حالت ECB هم رازآلود نیست: بلاک‌های یکسان ورودی، بلاک‌های یکسان خروجی می‌دهند (پنگوئن معروف ECB جالب‌ترین نمونه است). این دمو به‌صورت واقعی MD5 و SHA-256 را محاسبه و مقایسه می‌کند.",
            CodeSnippet = """
                // ❌ ممنوع (بند V6.2.5): MD5 و SHA-1 برای رمز عبور و امضا
                using var md5 = MD5.Create();
                var badHash = md5.ComputeHash(data);

                // ❌ ممنوع: حالت ECB در AES
                // new AesCryptoServiceProvider { Mode = CipherMode.ECB }

                // ✓ مجاز: SHA-256 برای هش معمولی، AES-GCM برای رمزنگاری
                var goodHash = SHA256.HashData(data);
                """,
            BackController = "Cryptography"
        };

        // ------------------------- اجرای واقعی دمو -------------------------
        string password = "MySecret-2024!";
        byte[] data = Encoding.UTF8.GetBytes(password);

        string md5Hash = Convert.ToHexString(MD5.HashData(data));
        string sha256Hash = Convert.ToHexString(SHA256.HashData(data));

        result.OutputLines.Add($"ورودی: {password}");
        result.OutputLines.Add("");
        result.OutputLines.Add($"MD5   (۳۲ کاراکتر - ممنوع بند V6.2.5): {md5Hash}");
        result.OutputLines.Add($"SHA-256 (۶۴ کاراکتر - استاندارد): {sha256Hash}");
        result.OutputLines.Add("");
        result.OutputLines.Add("چرا MD5 خطرناک است؟");
        result.OutputLines.Add("  • سرعت حمله Brute Force: روی کارت گرافیک مدرن، میلیاردها تلاش در ثانیه ممکن است");
        result.OutputLines.Add("  • Collision واقعی ثبت‌شده: می‌توان دو فایل متفاوت با MD5 یکسان ساخت (فریب امضای دیجیتال)");
        result.OutputLines.Add("");
        result.OutputLines.Add("نکته: حتی SHA-256 هم برای «رمز عبور» مناسب نیست (سریع است!)؛ برای رمز عبور باید از");
        result.OutputLines.Add("PBKDF2/bcrypt/Argon2id با Salt استفاده شود — همان چیزی که در دموی بخش V2 دیدید.");

        return View("../Shared/DemoResult", result);
    }

    // ------------------------------------------------------------------------
    // 🎯 دمو ۳: تولیدکننده عدد تصادفی رمزنگارانه (CSPRNG)
    //
    // ⚙️ بندهای استاندارد OWASP ASVS 4.0.3:
    //   V6.3.1 → همه اعداد تصادفی/توکن‌ها باید با CSPRNG تولید شوند
    //   V6.3.2 → GUIDها باید با نسخه ۴ و CSPRNG تولید شوند
    // ------------------------------------------------------------------------
    [HttpGet]
    public IActionResult RandomToken()
    {
        var result = new DemoResult
        {
            Title = "توکن تصادفی امن (CSPRNG) در برابر تصادف غیرامن",
            AsvsRef = "V6.3.1 — V6.3.2",
            AsvsSummary = "توکن‌ها و اعداد تصادفی امنیتی باید با تولیدکننده رمزنگارانه (CSPRNG) ساخته شوند.",
            Explanation = "کلاس Random در .NET «تصادفی» نیست؛ الگوریتمش قطعی (Deterministic) است و با دانستن Seed یا حتی چند خروجی قبلی، می‌توان خروجی‌های بعدی را حدس زد. اگر توکن بازنشانی رمز عبور با Random ساخته شود، مهاجم می‌تواند آن را پیش‌بینی کند! RandomNumberGenerator (CSPRNG) از منابع انتروپی سیستم‌عامل استفاده می‌کند و پیش‌بینی‌ناپذیر است. GUID در .NET هم نسخه ۴ و مبتنی بر CSPRNG است (بند V6.3.2).",
            CodeSnippet = """
                // ❌ غیرامن: Random قطعی و قابل حدس است
                var rnd = new Random(seed);
                var weakToken = rnd.Next(100000, 999999);

                // ✓ امن: CSPRNG (بند V6.3.1)
                byte[] token = RandomNumberGenerator.GetBytes(32);
                string hexToken = Convert.ToHexString(token);

                // ✓ GUID نسخه ۴ با CSPRNG (بند V6.3.2)
                var guid = Guid.NewGuid();
                """,
            BackController = "Cryptography"
        };

        // ------------------------- اجرای واقعی دمو -------------------------
        result.OutputLines.Add("【۱】 مشکل کلاس Random: قطعی بودن با Seed یکسان");
        var rnd1 = new Random(42);
        var rnd2 = new Random(42);
        var seq1 = string.Join(", ", Enumerable.Range(0, 6).Select(_ => rnd1.Next(0, 100)));
        var seq2 = string.Join(", ", Enumerable.Range(0, 6).Select(_ => rnd2.Next(0, 100)));
        result.OutputLines.Add($"   Random با Seed=42 (بار اول): {seq1}");
        result.OutputLines.Add($"   Random با Seed=42 (بار دوم): {seq2}");
        result.OutputLines.Add("   🔴 دو توالی کاملاً یکسان! اگر توکن بازیابی رمز با Random ساخته شود، قابل حدس است.");
        result.OutputLines.Add("");

        // ۳ توکن امن واقعی
        result.OutputLines.Add("【۲】 توکن‌های امن با RandomNumberGenerator (CSPRNG):");
        for (int i = 1; i <= 3; i++)
        {
            string token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            result.OutputLines.Add($"   توکن امن {i}: {token}");
        }
        result.OutputLines.Add("");
        var guid = Guid.NewGuid();
        result.OutputLines.Add($"【۳】 GUID نسخه ۴ (بند V6.3.2): {guid} ← حرف ۱۳ام آن «4» است (نسخه ۴)");
        result.OutputLines.Add("");
        result.OutputLines.Add("✓ توکن ۳۲ بایتی = ۲۵۶ بیت انتروپی؛ پیش‌بینی آن از نظر محاسباتی ناممکن است.");

        return View("../Shared/DemoResult", result);
    }

    // ------------------------------------------------------------------------
    // 🎯 دمو ۴: امضای پیام با HMAC برای تضمین یکپارچگی
    //
    // ⚙️ بندهای استاندارد OWASP ASVS 4.0.3:
    //   V6.2.7 → محافظت رمزداده با امضا، حالت احرازشده یا HMAC
    //   V6.2.8 → مقایسه‌های رمزنگارانه باید در زمان ثابت انجام شوند (ضد Timing Attack)
    // ------------------------------------------------------------------------
    [HttpGet]
    public IActionResult Hmac()
    {
        var result = new DemoResult
        {
            Title = "امضای پیام با HMAC-SHA256 و مقایسه در زمان ثابت",
            AsvsRef = "V6.2.7 — V6.2.8",
            AsvsSummary = "یکپارچگی داده باید با HMAC تأیید شود و مقایسه برچسب‌ها در زمان ثابت (Constant-Time) انجام گیرد.",
            Explanation = "HMAC (کد احراز پیام مبتنی بر هش) ثابت می‌کند که پیام «دست نخورده» است و از طرف دارنده کلید مخفی آمده. کاربرد واقعی: امضای Webhook، اعتبارسنجی توکن و کوکی. نکته مهم دوم: مقایسه ساده با == به محض اولین بایت متفاوت «متوقف می‌شود»؛ زمان واکنش به مهاجم می‌گوید چند بایت اول درست بوده (Timing Attack). تابع FixedTimeEquals همیشه با سرعت یکسان مقایسه می‌کند. در این دمو پیام دستکاری‌شده از تأیید HMAC رد می‌شود.",
            CodeSnippet = """
                // تولید امضای HMAC-SHA256 (بند V6.2.7)
                byte[] signature = HMACSHA256.HashData(key, message);

                // مقایسه امن در زمان ثابت (بند V6.2.8) — هرگز از == استفاده نکنید!
                bool valid = CryptographicOperations.FixedTimeEquals(
                    receivedSignature, expectedSignature);
                """,
            BackController = "Cryptography"
        };

        // ------------------------- اجرای واقعی دمو -------------------------
        byte[] key = Encoding.UTF8.GetBytes("webhook-secret-key-آزمایشی");
        string message = "order_id=1024&amount=250000";

        // امضای اصلی
        byte[] originalSignature = HMACSHA256.HashData(key, Encoding.UTF8.GetBytes(message));

        result.OutputLines.Add($"پیام: {message}");
        result.OutputLines.Add($"امضای HMAC اصلی (Base64): {Convert.ToBase64String(originalSignature)}");
        result.OutputLines.Add("");

        // تأیید پیام سالم
        bool valid1 = CryptographicOperations.FixedTimeEquals(
            originalSignature,
            HMACSHA256.HashData(key, Encoding.UTF8.GetBytes(message)));
        result.OutputLines.Add($"【۱】 تأیید پیام سالم: {(valid1 ? "✓ امضا معتبر است" : "✗")}");

        // دستکاری مهاجم: افزایش مبلغ سفارش!
        string tamperedMessage = "order_id=1024&amount=999999";
        bool valid2 = CryptographicOperations.FixedTimeEquals(
            originalSignature,
            HMACSHA256.HashData(key, Encoding.UTF8.GetBytes(tamperedMessage)));
        result.OutputLines.Add($"【۲】 مهاجم مبلغ را به 999999 تغییر می‌دهد: {tamperedMessage}");
        result.OutputLines.Add($"    تأیید پیام دستکاری‌شده: {(valid2 ? "❌ امضا معتبر شد!" : "⛔ امضا نامعتبر — پیام رد شد ✓")}");
        result.OutputLines.Add("");
        result.OutputLines.Add("نکته بند V6.2.8: مقایسه انجام‌شده با CryptographicOperations.FixedTimeEquals بود؛");
        result.OutputLines.Add("این تابع همیشه همه بایت‌ها را مقایسه می‌کند و از نشت اطلاعات زمانی جلوگیری می‌کند.");

        return View("../Shared/DemoResult", result);
    }
}
