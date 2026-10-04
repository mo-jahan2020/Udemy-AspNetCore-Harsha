using System.Security.Cryptography;
using System.Text;
using AsvsLab.Models;
using Microsoft.AspNetCore.Mvc;

namespace AsvsLab.Controllers;

// ============================================================================
// 📋 بخش ششم: مدیریت خطا و لاگ امنیتی
// فصل V7 استاندارد OWASP ASVS 4.0.3
// ============================================================================
public class ErrorLoggingController : Controller
{
    private readonly ILogger<ErrorLoggingController> _logger;

    public ErrorLoggingController(ILogger<ErrorLoggingController> logger)
    {
        _logger = logger;
    }

    // ------------------------------------------------------------------------
    // فهرست دموهای این بخش (صفحه اول کنترلر)
    // ------------------------------------------------------------------------
    private static readonly SectionCatalog Catalog = new()
    {
        Title = "بخش ۶: مدیریت خطا و لاگ امنیتی",
        ChapterRef = "فصل V7 — OWASP ASVS 4.0.3",
        Demos =
        {
            new() { Action = "SafeError", Title = "خطای عمومی + شناسه یکتا (بدون افشای Stack Trace)", AsvsRef = "V7.4.1 — V7.1.4" },
            new() { Action = "LogInjection", Title = "حمله Log Injection و خنثی‌سازی (فرم ورودی)", AsvsRef = "V7.3.1" },
            new() { Action = "NoSensitiveLogs", Title = "ممنوعیت ثبت داده حساس در لاگ", AsvsRef = "V7.1.1 — V7.1.2" },
        }
    };

    [HttpGet]
    public IActionResult Index() => View("../Shared/SectionIndex", Catalog);

    // ------------------------------------------------------------------------
    // 🎯 دمو ۱: نمایش پیام خطای عمومی + شناسه یکتا برای پشتیبانی
    //
    // ⚙️ بندهای استاندارد OWASP ASVS 4.0.3:
    //   V7.4.1 → هنگام خطای غیرمنتظره، پیام عمومی (اختیاراً با شناسه یکتا) نمایش داده شود
    //   V7.1.3 → رویدادهای امنیتی (ورود موفق/ناموفق، خطای کنترل دسترسی و...) لاگ شوند
    //   V7.1.4 → هر لاگ، اطلاعات لازم برای بررسی زمانی حادثه را داشته باشد
    // ------------------------------------------------------------------------
    [HttpGet]
    public IActionResult SafeError()
    {
        var result = new DemoResult
        {
            Title = "خطای عمومی + شناسه یکتا (به‌جای افشای Stack Trace)",
            AsvsRef = "V7.4.1 — V7.1.3 — V7.1.4",
            AsvsSummary = "خطای غیرمنتظره باید با پیام عمومی و شناسه یکتا نمایش داده شود و جزئیات کامل آن فقط در لاگ امنیتی ثبت گردد.",
            Explanation = "اگر Stack Trace مستقیماً به کاربر نشان داده شود، مهاجم نسخه فریم‌ورک، مسیرهای فایل، نام سرور و ساختار کد را کشف می‌کند و حمله بعدی دقیق‌تر می‌شود. روش استاندارد: یک شناسه یکتا (Error ID) برای همان خطا تولید کنید، جزئیات کامل را همراه همان شناسه در لاگ سرور بنویسید و به کاربر فقط بگویید «خطایی رخ داد با کد XXXX». کاربر آن کد را به پشتیبانی می‌دهد و پشتیبانی در لاگ، شرح کامل را می‌بیند.",
            CodeSnippet = """
                // تولید شناسه یکتا برای خطا (بند V7.4.1)
                var errorId = Guid.NewGuid();

                // ثبت جزئیات کامل فقط در لاگ سرور (بندهای V7.1.3 و V7.1.4)
                _logger.LogError(ex,
                    "خطای پردازش سفارش | ErrorId={ErrorId} | User={User} | Path={Path}",
                    errorId, User.Identity?.Name, HttpContext.Request.Path);

                // به کاربر فقط پیام عمومی + شناسه:
                return StatusCode(500, new {
                    Message = "خطایی رخ داد. لطفاً با پشتیبانی تماس بگیرید.",
                    ErrorId = errorId
                });
                """,
            BackController = "ErrorLogging"
        };

        // ------------------------- اجرای واقعی دمو -------------------------
        try
        {
            // شبیه‌سازی خطای غیرمنتظره در پردازش سفارش
            object? order = null;
            _ = order!.ToString(); // NullReferenceException
        }
        catch (Exception ex)
        {
            var errorId = Guid.NewGuid();

            // جزئیات کامل فقط در لاگ سرور (خروجی کنسول برنامه را ببینید)
            _logger.LogError(ex,
                "خطای پردازش سفارش | ErrorId={ErrorId} | User={User} | Path={Path} | Time={Time}",
                errorId, "ali", "/ErrorLogging/SafeError", DateTime.UtcNow);

            result.OutputLines.Add("تصور کنید این خطا در حین پردازش سفارش رخ داده است:");
            result.OutputLines.Add("");
            result.OutputLines.Add("【۱】 روش غلط (افشای جزئیات به کاربر):");
            result.OutputLines.Add($"   ❌ Stack Trace کامل: {ex.GetType().Name}: {ex.Message}");
            result.OutputLines.Add("   ❌ + مسیر فایل‌ها، نام متدها، نسخه فریم‌ورک و سرور...");
            result.OutputLines.Add("   مهاجم از این اطلاعات نقشه حمله بعدی را می‌کشد!");
            result.OutputLines.Add("");
            result.OutputLines.Add("【۲】 روش استاندارد (بند V7.4.1):");
            result.OutputLines.Add($"   ✓ پیام به کاربر: «خطایی غیرمنتظره رخ داد. با پشتیبانی تماس بگیرید.»");
            result.OutputLines.Add($"   ✓ شناسه خطا (ErrorId): {errorId}");
            result.OutputLines.Add($"   ✓ جزئیات کامل با همین شناسه در لاگ سرور ثبت شد (کنسول برنامه را ببینید)");
            result.OutputLines.Add("");
            result.OutputLines.Add("پشتیبانی با جستجوی ErrorId در لاگ، دقیقاً همان لحظه، کاربر و مسیر را می‌بیند (V7.1.4).");
        }

        return View("../Shared/DemoResult", result);
    }

    // ------------------------------------------------------------------------
    // 🎯 دمو ۲ (فرم ورودی): مقابله با حمله Log Injection
    //
    // ⚙️ بند استاندارد OWASP ASVS 4.0.3:
    //   V7.3.1 → اجزای لاگ‌گیری باید داده‌ها را انکد کنند تا از Log Injection
    //            جلوگیری شود
    // ------------------------------------------------------------------------
    [HttpGet]
    public IActionResult LogInjection(string? input)
    {
        const string asvsRef = "V7.3.1";
        const string summary = "داده‌های ورودی قبل از نوشتن در لاگ باید انکد/پاک‌سازی شوند تا مهاجم نتواند رکوردهای لاگ جعلی درج کند.";

        if (string.IsNullOrEmpty(input))
        {
            return View("../Shared/DemoForm", new DemoFormViewModel
            {
                Title = "حمله Log Injection (درج رکورد جعلی در لاگ)",
                AsvsRef = asvsRef,
                AsvsSummary = summary,
                Description = "فرض کنید لاگ لاگین به شکل «LOGIN OK: {نام‌کاربری}» نوشته می‌شود. مهاجم در نام کاربری خودش یک کاراکتر خط جدید (\\n) و سپس یک رکورد جعلی تزریق می‌کند. این متن را امتحان کنید: ali\nLOGIN FAILED: admin (پس از زدن دکمه، هر دو خط در لاگ ظاهر می‌شوند و کارشناس امنیت فریب می‌خورد.)",
                InputLabel = "نام کاربری برای لاگ",
                FormAction = "/ErrorLogging/LogInjection",
                PresetValue = "ali\\nLOGIN FAILED: admin"
            });
        }

        var result = new DemoResult
        {
            Title = "حمله Log Injection (درج رکورد جعلی در لاگ)",
            AsvsRef = asvsRef,
            AsvsSummary = summary,
            Explanation = "اگر ورودی کاربر بدون پاک‌سازی در فایل لاگ نوشته شود، مهاجم با کاراکتر CRLF (\\r\\n) یک «خط جدید» داخل لاگ درست می‌کند و رکورد جعلی می‌نویسد؛ مثلاً لاگ فریب‌دهنده از مهاجم دیگران، یا حتی اجرای الگوسازهای خودکار (Like ELK) را منحرف می‌کند. راه‌حل: حذف/جایگزینی کاراکترهای کنترلی (CR/LF) قبل از نوشتن، یا استفاده از لاگرهای ساختاریافته (Structured Logging) که مقدارها را جدا از متن رخداد ذخیره می‌کنند.",
            CodeSnippet = """
                // ❌ آسیب‌پذیر: نوشتن خام ورودی در لاگ
                _logger.LogInformation("LOGIN OK: " + username);

                // ✓ امن (بند V7.3.1): خنثی‌سازی کاراکترهای کنترلی
                var safe = username
                    .Replace("\r", "<CR>").Replace("\n", "<LF>");
                _logger.LogInformation("LOGIN OK: {Username}", safe);

                // ✓ حتی بهتر: لاگ ساختاریافته (مقدار جدا از متن)
                _logger.LogInformation("LOGIN_OK {Username}", username);
                """,
            BackController = "ErrorLogging"
        };

        // برای نمایش حمله، \n متنی را به خط واقعی تبدیل می‌کنیم (شبیه‌سازی ورودی مهاجم)
        string attackerInput = input.Replace("\\n", "\n").Replace("\\r", "\r");

        result.OutputLines.Add($"ورودی خام مهاجم: {attackerInput.Replace("\n", "\\n").Replace("\r", "\\r")}");
        result.OutputLines.Add("");
        result.OutputLines.Add("【۱】 لاگ آسیب‌پذیر (نوشتن خام ورودی):");
        foreach (var line in attackerInput.Split('\n'))
            result.OutputLines.Add($"   | {line.TrimEnd('\r')}");
        result.OutputLines.Add("   🔴 دو رکورد در لاگ ثبت شد؛ رکورد دوم کاملاً جعلی است!");
        result.OutputLines.Add("");

        // نسخه امن: خنثی‌سازی کاراکترهای کنترلی
        string safeInput = attackerInput.Replace("\r", "<CR>").Replace("\n", "<LF>");
        result.OutputLines.Add("【۲】 لاگ امن (خنثی‌سازی CRLF - بند V7.3.1):");
        result.OutputLines.Add($"   | LOGIN OK: {safeInput}");
        result.OutputLines.Add("   ✓ یک رکورد؛ تزریق خنثی شد و متن جعلی فقط به‌صورت قابل‌مشاهده ثبت شد.");

        return View("../Shared/DemoResult", result);
    }

    // ------------------------------------------------------------------------
    // 🎯 دمو ۳: ممنوعیت ثبت داده‌های حساس در لاگ
    //
    // ⚙️ بندهای استاندارد OWASP ASVS 4.0.3:
    //   V7.1.1 → اعتبارنامه‌ها و داده‌های پرداخت نباید در لاگ نوشته شوند
    //   V7.1.2 → داده‌های حساس تعریف‌شده در قوانین حفظ حریم خصوصی هم نباید لاگ شوند
    // ------------------------------------------------------------------------
    [HttpGet]
    public IActionResult NoSensitiveLogs()
    {
        var result = new DemoResult
        {
            Title = "ممنوعیت ثبت رمز عبور و داده حساس در لاگ",
            AsvsRef = "V7.1.1 — V7.1.2",
            AsvsSummary = "اعتبارنامه‌ها (رمز عبور، توکن، کارت بانکی) و داده‌های حساس هرگز نباید در لاگ‌ها نوشته شوند؛ توکن نشست هم فقط به‌صورت هش نامعکوس.",
            Explanation = "لاگ‌ها معمولاً دسترسی گسترده دارند (پشتیبان‌ها، ابزارهای مانیتورینگ، تیم‌های متعدد) و اغلب رمزنگاری نمی‌شوند؛ اگر رمز عبور یا شماره کارت در لاگ بنویسد، عملاً یک «کپی از اعتبارنامه‌های همه کاربران» روی دیسک ساخته‌اید. راه‌حل: قبل از لاگ، فیلدهای حساس را ماسک کنید (فقط ۴ رقم آخر) و در Code Review چک‌لیست ممنوعیت لاگ داده حساس داشته باشید. برای توکن نشست، هش (SHA-256) ذخیره شود تا در صورت لو رفتن لاگ، توکن قابل استفاده نباشد.",
            CodeSnippet = """
                // ❌ ممنوع (بند V7.1.1): اعتبارنامه در لاگ
                _logger.LogInformation("ورود با رمز: {Password}", password);

                // ✓ امن: ماسک کردن داده حساس
                _logger.LogInformation("کارت: ****-****-****-{Last4}",
                    cardNumber[^4..]);

                // ✓ توکن نشست فقط به‌صورت هش نامعکوس (بند V7.1.1)
                var tokenHash = Convert.ToHexString(
                    SHA256.HashData(Encoding.UTF8.GetBytes(sessionToken)));
                _logger.LogInformation("توکن (هش): {TokenHash}", tokenHash);
                """,
            BackController = "ErrorLogging"
        };

        // ------------------------- اجرای واقعی دمو -------------------------
        string password = "SuperSecret!2024";
        string card = "6104337812345678";
        string sessionToken = "CfDJ8N4k3T9xKbXa...";

        result.OutputLines.Add("【۱】 لاگ غلط (داده حساس خام):");
        result.OutputLines.Add($"   ❌ _logger.Log(\"ورود موفق: کاربر ali با رمز {password}\")");
        result.OutputLines.Add("   🔴 رمز خام اکنون روی دیسک، در بکاپ‌ها و ابزار مانیتورینگ است!");
        result.OutputLines.Add("");

        string maskedCard = $"****-****-****-{card[^4..]}";
        string tokenHash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(sessionToken)));

        result.OutputLines.Add("【۲】 لاگ امن (ماسک + هش):");
        result.OutputLines.Add($"   ✓ کارت: {maskedCard}");
        result.OutputLines.Add($"   ✓ توکن نشست (هش SHA-256): {tokenHash[..32]}...");
        result.OutputLines.Add("");
        result.OutputLines.Add("اگر فایل لاگ لو برود، مهاجم فقط ۴ رقم آخر کارت و هشِ بازگشت‌ناپذیر توکن را می‌بیند.");
        result.OutputLines.Add("نکته: همین لاگ‌های امن در کنسول این برنامه ثبت شد (ILogger) — می‌توانید ببینید.");

        return View("../Shared/DemoResult", result);
    }
}
