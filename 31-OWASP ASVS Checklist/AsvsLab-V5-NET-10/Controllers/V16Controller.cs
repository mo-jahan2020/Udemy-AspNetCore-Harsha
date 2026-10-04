using System.Text.RegularExpressions;
using AsvsLab.Services;
using Microsoft.AspNetCore.Mvc;

namespace AsvsLab.Controllers;

// ═══════════════════════════════════════════════════════════════════
//  فصل V16 — لاگ امنیتی و مدیریت خطا (Security Logging and Error Handling)
// ═══════════════════════════════════════════════════════════════════
public class V16Controller : DemoBaseController
{
    private readonly ILogger<V16Controller> _logger;
    public V16Controller(AsvsRefService asvs, ProgressService progress, ILogger<V16Controller> logger)
        : base(asvs, progress) => _logger = logger;

    public IActionResult Index() =>
        SectionIndex(16, "لاگ و خطا: پیام خطای عمومی، لاگ بدون داده حساس، ضد تزریق لاگ، زمان UTC و ثبت تلاش‌های مجوز. لاگ واقعی کنسول برنامه را هنگام دموها ببینید!");

    // ─────────────────────────────────────────────────────────────
    // دموی ۱: پیام خطای عمومی — بند V16.5.1
    // ─────────────────────────────────────────────────────────────
    public IActionResult GenericError()
    {
        var r = New("v16-generic", "V16.5.1",
            "در خطای غیرمنتظره، پیام عمومی برگردانده شود؛ جزئیات داخلی فقط در لاگ امن می‌رود.", "V16");
        r.VulnerableCode = """
            // ❌ ناامن: برگرداندن استثنا به کاربر
            try { ProcessPayment(); }
            catch (Exception ex) { return Content(ex.ToString()); }
            // Stack Trace = نقشه کامل حمله برای مهاجم
            """;
        r.SecureCode = """
            // ✅ امن: پیام عمومی به کاربر، جزئیات به لاگ
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطای پرداخت برای سفارش {OrderId}", orderId);
                return View("Error", "خطای غیرمنتظره — لطفاً دوباره تلاش کنید");
            }
            """;
        try
        {
            throw new InvalidOperationException(
                "Server=10.0.0.5;Password=s3cret — جزئیات داخلی که نباید بیرون برود");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "دموی V16: خطای پرداخت شبیه‌سازی شد (جزئیات فقط اینجا در لاگ کنسول است)");
            r.OutputLines.Add("❌ چیزی که مهاجم با کد ناامن می‌بیند:");
            r.OutputLines.Add($"   {ex.Message}");
            r.OutputLines.Add("✅ چیزی که با کد امن کاربر می‌بیند: «خطای غیرمنتظره — لطفاً دوباره تلاش کنید»");
            r.OutputLines.Add("✅ و جزئیات کامل فقط در لاگ کنسول برنامه ثبت شد (همین الان نگاه کنید!).");
        }
        return View("../Shared/DemoResult", r);
    }

    // ─────────────────────────────────────────────────────────────
    // دموی ۲: لاگ بدون داده حساس — بند V16.2.5
    // ─────────────────────────────────────────────────────────────
    public IActionResult SafeLogging()
    {
        var r = New("v16-safe", "V16.2.5",
            "ثبت داده حساس طبق سطح محافظت کنترل شود؛ اعتبارنامه هرگز در لاگ نمی‌آید.", "V16");
        r.VulnerableCode = """
            // ❌ ناامن: کل درخواست در لاگ
            _logger.LogInformation("درخواست ورود: {Body}", body);
            // "password=Ali12345!" برای همیشه در فایل لاگ ماند!
            """;
        r.SecureCode = """
            // ✅ امن: ماسک‌سازی قبل از لاگ
            var safe = Mask(body, "password", "card");
            _logger.LogInformation("درخواست ورود: {Body}", safe);
            """;
        var raw = "username=ali&password=Ali12345!&card=6037991112345678";
        var masked = Regex.Replace(raw, @"(?<=(password|card)=)[^&]+", m => new string('*', m.Value.Length));
        _logger.LogInformation("دموی V16: رشته لاگ‌شده امن: {Safe}", masked);

        r.OutputLines.Add($"رشته خام (هرگز لاگ نمی‌شود): {raw}");
        r.OutputLines.Add($"رشته ماسک‌شده برای لاگ:    {masked}");
        r.OutputLines.Add("قواعد: گذرواژه کامل ستاره؛ شماره کارت فقط ۴ رقم آخر؛ CVV اصلاً ذخیره نمی‌شود (PCI DSS).");
        r.OutputLines.Add("بندهای مرتبط: V16.3.2 (لاگ رویدادهای مجوز) و V14.2.6 (حداقل داده لازم).");
        return View("../Shared/DemoResult", r);
    }

    // ─────────────────────────────────────────────────────────────
    // دموی ۳: ضد تزریق لاگ — بند V16.4.1
    // ─────────────────────────────────────────────────────────────
    public IActionResult LogInjection()
    {
        var r = New("v16-inject", "V16.4.1",
            "داده‌های لاگ باید انکد/پاک‌سازی شوند تا تزریق لاگ (جعل ردیف) ممکن نشود.", "V16");
        r.VulnerableCode = """
            // ❌ ناامن: ورودی خام در لاگ
            _logger.LogInformation("جستجوی کاربر: {Q}", userInput);
            // userInput = "ali\n2026-09-09 ERROR admin login failed"
            // → ردیف جعلی ERROR در لاگ شما!
            """;
        r.SecureCode = """
            // ✅ امن: نویسه‌های کنترلی CRLF خنثی می‌شوند
            var safe = userInput.Replace("\r", "_").Replace("\n", "_");
            _logger.LogInformation("جستجوی کاربر: {Q}", safe);
            """;
        var evil = "ali\n2026-09-09 10:00:00 ERROR [admin] login FAILED from 10.0.0.66";
        var safe = evil.Replace("\r", "_").Replace("\n", "_");
        _logger.LogInformation("دموی V16: ورودی مهاجم ماسک شد: {Safe}", safe);
        r.OutputLines.Add("ورودی مهاجم (خام):");
        r.OutputLines.Add($"   {evil.Replace("\n", "⏎")}");
        r.OutputLines.Add("هدف مهاجم: تزریق یک ردیف ERROR جعلی برای گمراهی تیم امنیت/پاک شدن ردپای خودش.");
        r.OutputLines.Add($"ورودی پس از خنثی‌سازی CRLF: {safe}");
        return View("../Shared/DemoResult", r);
    }

    // ─────────────────────────────────────────────────────────────
    // دموی ۴: زمان UTC در لاگ — بند V16.2.2
    // ─────────────────────────────────────────────────────────────
    public IActionResult UtcTimestamp()
    {
        var r = New("v16-utc", "V16.2.2",
            "زمان‌سنجی اجزای لاگ همگام و با UTC (یا آفست صریح) باشد.", "V16");
        var now = DateTime.Now;
        var utc = DateTime.UtcNow;
        _logger.LogInformation("دموی V16: رویداد در UTC ثبت شد: {Ts:o}", utc);
        r.OutputLines.Add($"DateTime.Now  (موقتی — برای لاگ مناسب نیست): {now:yyyy-MM-dd HH:mm:ss zzz}");
        r.OutputLines.Add($"DateTime.UtcNow (استاندارد لاگ):                    {utc:yyyy-MM-dd HH:mm:ss 'Z'}");
        r.OutputLines.Add("چرا UTC؟ هنگام بررسی حمله در چند سرور/کشور، مرتب‌سازی زمانی بی‌معنا می‌شود اگر هر سرور زمان محلی بنویسد.");
        r.OutputLines.Add("در لاگ کنسول برنامه همین حالا timestamp UTC ILogger را می‌بینید.");
        return View("../Shared/DemoResult", r);
    }

    // ─────────────────────────────────────────────────────────────
    // دموی متنی: لاگ رویدادهای مجوز — بند V16.3.2
    // ─────────────────────────────────────────────────────────────
    public IActionResult AuthzLogging() =>
        View("../Shared/DemoResult", TextDemo("v16-authz", "V16.3.2",
            "تلاش‌های ناموفق مجوز (و در سطح ۳ همه تصمیم‌های مجوز) باید لاگ شوند — بدون داده حساس.", "V16",
            "رویدادهای حداقلی برای هر تصمیم مجوز:\n" +
            "• زمان UTC، هویت (شناسه — نه داده شخصی حساس)، منبع درخواستی، تصمیم (Allow/Deny)، IP\n\n" +
            "این برنامه همین حالا انجام می‌دهد:\n" +
            "• AccountController: لاگ Warning تلاش ورود ناموفق (متن امنیتی، بدون گذرواژه)\n" +
            "• [Authorize] رد شده‌ها با StatusCodePages 403 قابل ردیابی‌اند.\n\n" +
            "الگوی تکمیلی برای سطح ۳:\n" +
            "_logger.LogInformation(\"Authz: user={UserId} resource={Res} decision={D}\", id, res, allowed);\n" +
            "به همراه لاگ دسترسی به داده حساس (مثل مشاهده فاکتور) با شناسه — نه محتوا.\n\n" +
            "پیوند به بند V16.1.1: فهرستِ اینکه هر لایه چه چیزی لاگ می‌کند باید مستند باشد؛ " +
            "اجرای کامل SIEM خارج از محدوده این اپ آموزشی است و متنی ارائه شد."));
}
