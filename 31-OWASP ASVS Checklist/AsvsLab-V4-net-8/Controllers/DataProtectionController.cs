using System.Security.Cryptography;
using System.Text;
using AsvsLab.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;

namespace AsvsLab.Controllers;

// ============================================================================
// 🛡️ بخش هفتم: محافظت از داده‌ها (Data Protection)
// فصل V8 استاندارد OWASP ASVS 4.0.3
// ============================================================================
public class DataProtectionController : Controller
{
    private readonly IDataProtectionProvider _provider;

    public DataProtectionController(IDataProtectionProvider provider)
    {
        _provider = provider;
    }

    // ------------------------------------------------------------------------
    // فهرست دموهای این بخش (صفحه اول کنترلر)
    // ------------------------------------------------------------------------
    private static readonly SectionCatalog Catalog = new()
    {
        Title = "بخش ۷: محافظت از داده‌ها",
        ChapterRef = "فصل V8 — OWASP ASVS 4.0.3",
        Demos =
        {
            new() { Action = "DataProtectionApi", Title = "رمزنگاری با Data Protection API و جداسازی Purpose", AsvsRef = "V8.3.7 — V6.4.1" },
            new() { Action = "NoCache", Title = "جلوگیری از کش شدن داده حساس در مرورگر", AsvsRef = "V8.2.1 — V8.2.2" },
            new() { Action = "PiiStorage", Title = "ذخیره امن کد ملی: رمزنگاری یا هش؟", AsvsRef = "V8.3.4 — V6.1.1" },
        }
    };

    [HttpGet]
    public IActionResult Index() => View("../Shared/SectionIndex", Catalog);

    // ------------------------------------------------------------------------
    // 🎯 دمو ۱: رمزنگاری احرازشده با API محافظت از داده ASP.NET Core
    //
    // ⚙️ بندهای استاندارد OWASP ASVS 4.0.3:
    //   V8.3.7 → اطلاعات حساس باید با الگوریتم‌های تأییدشده که محرمانگی و
    //            یکپارچگی را همزمان فراهم کنند رمزنگاری شوند
    //   V6.4.1 → کلیدها باید به‌صورت امن مدیریت شوند (ایجاد، چرخش، نابودی)
    // ------------------------------------------------------------------------
    [HttpGet]
    public IActionResult DataProtectionApi()
    {
        var result = new DemoResult
        {
            Title = "رمزنگاری داده حساس با Data Protection API",
            AsvsRef = "V8.3.7 — V6.4.1",
            AsvsSummary = "داده حساس با الگوریتم‌های دارای محرمانگی + یکپارچگی رمز شود و مدیریت کلید به‌صورت امن انجام گیرد.",
            Explanation = "سرویس Data Protection همان موتوری است که کوکی‌های احراز هویت ASP.NET Core را رمز می‌کند: رمزنگاری احرازشده (AES + HMAC)، مدیریت خودکار چرخش کلید و مفهوم «Purpose» — یعنی رمزی که برای «فایل‌های موقت» ساخته شده، برای «کوکی احراز هویت» قابل استفاده نیست؛ این جداسازی، دامنه آسیب را محدود می‌کند. در این دمو رشته کوت‌مدت (OTC) را رمز و بازگشایی می‌کنیم و می‌بینیم که با Purpose غلط، رمزگشایی شکست می‌خورد.",
            CodeSnippet = """
                // رمزنگاری با Purpose مشخص (بند V8.3.7)
                var protector = _provider.CreateProtector(
                    "AsvsLab.OneTimeCode.v1");
                string encrypted = protector.Protect("کد-محرمات-123");

                // رمزگشایی فقط با همان Purpose ممکن است:
                string plain = protector.Unprotect(encrypted);

                // با Purpose دیگر → خطای CryptographicException (جداسازی دامنه)
                var otherProtector = _provider.CreateProtector(
                    "AsvsLab.Other.Purpose");
                otherProtector.Unprotect(encrypted); // ❌ شکست می‌خورد
                """,
            BackController = "DataProtection"
        };

        // ------------------------- اجرای واقعی دمو -------------------------
        string sensitive = "کد بازیابی دو مرحله‌ای: 558210";

        var protector = _provider.CreateProtector("AsvsLab.OneTimeCode.v1");
        string encrypted = protector.Protect(sensitive);
        string decrypted = protector.Unprotect(encrypted);

        result.OutputLines.Add($"داده اصلی: {sensitive}");
        result.OutputLines.Add($"رمزشده (Base64): {encrypted[..50]}...");
        result.OutputLines.Add($"بازگشایی با همان Purpose: {decrypted} ✓");
        result.OutputLines.Add("");

        // تلاش با Purpose غلط → شکست
        result.OutputLines.Add("مهاجم همین رشته رمزشده را با Purpose دیگری بازگشایی می‌کند:");
        try
        {
            var wrongProtector = _provider.CreateProtector("AsvsLab.Other.Purpose");
            var hacked = wrongProtector.Unprotect(encrypted);
            result.OutputLines.Add($"   ❌ بازگشایی شد: {hacked}");
        }
        catch (System.Security.Cryptography.CryptographicException)
        {
            result.OutputLines.Add("   ⛔ خطای CryptographicException — با Purpose دیگر قابل بازگشایی نیست ✓");
            result.OutputLines.Add("   (کلید جداسازی Purpose باعث محدود شدن دامنه آسیب می‌شود)");
        }
        result.OutputLines.Add("");
        result.OutputLines.Add("نکته (بند V6.4.1): کلیدهای Data Protection به‌طور خودکار چرخش می‌شوند؛");
        result.OutputLines.Add("در Production محل ذخیره کلیدها را به‌صورت امن پیکربندی کنید (PersistKeysToAzureBlobStorage و...).");

        return View("../Shared/DemoResult", result);
    }

    // ------------------------------------------------------------------------
    // 🎯 دمو ۲: جلوگیری از کش شدن داده حساس در مرورگر
    //
    // ⚙️ بند استاندارد OWASP ASVS 4.0.3:
    //   V8.2.1 → هدرهای ضد-کش کافی تنظیم شود تا داده حساس در مرورگر کش نشود
    //   V8.2.2 → داده حساس نباید در localStorage/ذخیره‌گاه مرورگر ذخیره شود
    // ------------------------------------------------------------------------
    [HttpGet]
    public IActionResult NoCache()
    {
        var result = new DemoResult
        {
            Title = "جلوگیری از کش شدن داده حساس در مرورگر",
            AsvsRef = "V8.2.1 — V8.2.2",
            AsvsSummary = "پاسخ‌های حاوی داده حساس باید با هدرهای ضد-کش ارسال شوند و داده حساس در ذخیره‌گاه مرورگر ذخیره نشود.",
            Explanation = "اگر صفحه‌ای که «صورت‌حساب» یا «اطلاعات کارت» را نشان می‌دهد کش شود، کاربر بعدی روی همان کامپیوتر می‌تواند با Back یا تاریخچه مرورگر، داده فرد قبلی را ببیند؛ این در کافی‌نت‌ها و کامپیوترهای اشتراکی یک ریسک واقعی است. راه‌حل: هدر Cache-Control: no-store, no-cache + Pragma: no-cache روی پاسخ‌های حساس. در این دمو این هدرها را به‌صورت واقعی روی پاسخ جاری تنظیم می‌کنیم (با F12 در تب Network قابل مشاهده است).",
            CodeSnippet = """
                // روی Action حساس (بند V8.2.1):
                Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
                Response.Headers["Pragma"] = "no-cache";
                Response.Headers["Expires"] = "0";

                // یا به‌صورت سراسری با [ResponseCache] / فیلتر سفارشی
                """,
            BackController = "DataProtection"
        };

        // ------------------------- اجرای واقعی دمو -------------------------
        // تنظیم واقعی هدرهای ضد-کش روی همین پاسخ (F12 → Network → Headers)
        Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
        Response.Headers["Pragma"] = "no-cache";
        Response.Headers["Expires"] = "0";

        result.OutputLines.Add("هدرهای ضد-کش روی همین پاسخ HTTP تنظیم شد:");
        result.OutputLines.Add($"   Cache-Control: {Response.Headers["Cache-Control"]}");
        result.OutputLines.Add($"   Pragma: {Response.Headers["Pragma"]}");
        result.OutputLines.Add($"   Expires: {Response.Headers["Expires"]}");
        result.OutputLines.Add("");
        result.OutputLines.Add("برای مشاهده واقعی: کلید F12 → تب Network → این درخواست → Response Headers");
        result.OutputLines.Add("");
        result.OutputLines.Add("تذکر بند V8.2.2: داده حساس (توکن، پن، کد ملی) هرگز نباید در localStorage یا");
        result.OutputLines.Add("sessionStorage ذخیره شود؛ این ذخیره‌گاه‌ها به XSS کاملاً آسیب‌پذیرند.");

        return View("../Shared/DemoResult", result);
    }

    // ------------------------------------------------------------------------
    // 🎯 دمو ۳: طبقه‌بندی داده — رمزنگاری در برابر هش برای کد ملی
    //
    // ⚙️ بندهای استاندارد OWASP ASVS 4.0.3:
    //   V8.3.4 → داده‌های حساس شناسایی و سیاست مدیریت آن‌ها تعریف شود
    //   V6.1.1 → داده شخصی (PII) باید در حالت سکون رمزنگاری شود
    // ------------------------------------------------------------------------
    [HttpGet]
    public IActionResult PiiStorage()
    {
        var result = new DemoResult
        {
            Title = "ذخیره امن کد ملی (PII): رمزنگاری در برابر هش Salting‌شده",
            AsvsRef = "V8.3.4 — V6.1.1",
            AsvsSummary = "داده شخصی باید شناسایی، طبقه‌بندی و بر اساس نیاز (نمایش یا فقط جستجو) رمزنگاری یا هش شود.",
            Explanation = "برای هر داده حساس ابتدا بپرسید «آیا باید دوباره «نمایش» داده شود یا فقط «جستجو/مقایسه»؟». اگر باید نمایش داده شود → رمزنگاری معکوس‌پذیر (AES-GCM) با کلید مدیریت‌شده. اگر فقط برای جستجو/یکتایی لازم است (مثلاً جلوگیری از ثبت‌نام تکراری) → هش با Salt اختصاصی؛ حتی اگر دیتابیس کامل لو برود، مقدار اصلی بازیابی نمی‌شود. در این دمو هر دو روش واقعاً اجرا می‌شود.",
            CodeSnippet = """
                // سناریو ۱: باید بعداً نمایش داده شود → رمزنگاری معکوس‌پذیر (V6.1.1)
                var protector = _provider.CreateProtector("PII.NationalCode.v1");
                string stored = protector.Protect(nationalCode);

                // سناریو ۲: فقط جستجو/یکتایی → هش Salt‌دار (غیرمعکوس)
                var salt = RandomNumberGenerator.GetBytes(16);
                var hash = Rfc2898DeriveBytes.Pbkdf2(nationalCode, salt, 100_000,
                    HashAlgorithmName.SHA256, 32);
                """,
            BackController = "DataProtection"
        };

        // ------------------------- اجرای واقعی دمو -------------------------
        string nationalCode = "0079123456";

        // سناریو ۱: رمزنگاری معکوس‌پذیر
        var protector = _provider.CreateProtector("PII.NationalCode.v1");
        string encrypted = protector.Protect(nationalCode);
        string decrypted = protector.Unprotect(encrypted);

        // سناریو ۲: هش با Salt اختصاصی
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(nationalCode, salt, 100_000, HashAlgorithmName.SHA256, 32);

        result.OutputLines.Add($"کد ملی کاربر: {nationalCode}");
        result.OutputLines.Add("");
        result.OutputLines.Add("【۱】 ذخیره رمزنگاری‌شده (وقتی باید بعداً نمایش دهید):");
        result.OutputLines.Add($"   رمزشده: {encrypted[..46]}...");
        result.OutputLines.Add($"   بازیابی: {protector.Unprotect(encrypted)} ✓");
        result.OutputLines.Add("");
        result.OutputLines.Add("【۲】 ذخیره هش Salt‌دار (وقتی فقط برای جستجو/جلوگیری از تکرار لازم است):");
        result.OutputLines.Add($"   Salt: {Convert.ToBase64String(salt)}");
        result.OutputLines.Add($"   Hash: {Convert.ToHexString(hash)[..32]}...");
        result.OutputLines.Add("   از این مقدار نمی‌توان کد ملی اصلی را برگرداند ✓");
        result.OutputLines.Add("");
        result.OutputLines.Add("✓ اگر دیتابیس کامل سرقت شود، در روش دوم کد ملی به هیچ شکلی قابل بازیابی نیست.");

        return View("../Shared/DemoResult", result);
    }
}
