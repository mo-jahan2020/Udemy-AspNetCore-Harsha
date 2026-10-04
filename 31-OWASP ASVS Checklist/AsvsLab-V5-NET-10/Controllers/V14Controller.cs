using AsvsLab.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;

namespace AsvsLab.Controllers;

// ═══════════════════════════════════════════════════════════════════
//  فصل V14 — محافظت از داده‌ها (Data Protection)
//  در ASVS 5.0 محتوای فصل V8 قدیم (4.0.3) اینجاست؛ Data Protection
//  دات‌نت برای «رمزنگاری در سکون» به‌صورت واقعی اجرا می‌شود
// ═══════════════════════════════════════════════════════════════════
public class V14Controller : DemoBaseController
{
    private readonly IDataProtectionProvider _dp;
    public V14Controller(AsvsRefService asvs, ProgressService progress, IDataProtectionProvider dp)
        : base(asvs, progress) => _dp = dp;

    public IActionResult Index() =>
        SectionIndex(14, "محافظت داده: طبقه‌بندی، ممنوعیت داده حساس در URL، رمزنگاری در سکون (دموی واقعی با Data Protection)، هدرهای ضدکش و پاک‌سازی پس از نشست.");

    // ─────────────────────────────────────────────────────────────
    // دموی ۱: رمزنگاری در سکون — بند V14.2.4 (اجرای واقعی)
    // ─────────────────────────────────────────────────────────────
    public IActionResult DpAtRest()
    {
        var r = New("v14-dp", "V14.2.4",
            "داده حساس در حالت سکون باید رمزنگاری و از نظر یکپارچگی محافظت شود.", "V14");
        r.VulnerableCode = """
            // ❌ ناامن: ذخیره توکن/داده حساس به‌صورت Base64 یا متن آشکار
            var stored = Convert.ToBase64String(Encoding.UTF8.GetBytes(apiToken));
            await File.WriteAllTextAsync("tokens.dat", stored);
            // Base64 «رمزنگاری» نیست — هر کسی دیکود می‌کند!
            """;
        r.SecureCode = """
            // ✅ امن: Data Protection با purpose مشخص (همین برنامه)
            var protector = _dp.CreateProtector("AsvsLab.V14.UserTokens");
            var protected_ = protector.Protect(apiToken);   // رمز + امضا
            var original   = protector.Unprotect(protected_); // دستکاری → استثنا
            // کلیدها خودکار چرخش می‌شوند (بند V11.1.1)
            """;
        var protector = _dp.CreateProtector("AsvsLab.V14.Demo");
        var secret = "کد بازنشانی: 918273645 (حساس!)";
        var protectedData = protector.Protect(secret);

        var back = protector.Unprotect(protectedData);
        var tampered = (protectedData[..^8] + "AAAAAAAA");
        string tamperResult;
        try { protector.Unprotect(tampered); tamperResult = "پذیرفته شد (هرگز نباید رخ دهد!)"; }
        catch (System.Security.Cryptography.CryptographicException) { tamperResult = "🚫 CryptographicException — دستکاری کشف شد"; }

        r.OutputLines.Add($"داده اصلی: {secret}");
        r.OutputLines.Add($"محافظت‌شده: {protectedData}");
        r.OutputLines.Add($"بازیابی: ✅ «{back}»");
        r.OutputLines.Add($"بازیابی بعد از دستکاری: {tamperResult}");
        r.OutputLines.Add("مکان کلیدها: پروفایل کاربری (در ویندوز: %LOCALAPPDATA%\\ASP.NET\\DataProtection-Keys) با چرخش خودکار ۹۰ روزه.");
        return View("../Shared/DemoResult", r);
    }

    // ─────────────────────────────────────────────────────────────
    // دموی ۲: هدرهای ضدکش — بند V14.3.2 (اجرای واقعی روی این پاسخ)
    // ─────────────────────────────────────────────────────────────
    public IActionResult AntiCaching()
    {
        var r = New("v14-cache", "V14.3.2",
            "پاسخ‌های حاوی داده حساس باید با هدرهای ضدکش از ذخیره در کش‌ها جلوگیری کنند.", "V14");
        r.VulnerableCode = """
            // ❌ ناامن: پاسخ پروفایل بدون هدر ضدکش
            return View(profile); // در proxy/مرورگر کش می‌شود؛ کاربر بعدی می‌بیند!
            """;
        r.SecureCode = """
            // ✅ امن (واقعاً روی همین پاسخ ست شده):
            Cache-Control: no-store, no-cache, must-revalidate
            Pragma: no-cache
            """;
        Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
        Response.Headers["Pragma"] = "no-cache";
        Response.Headers["Expires"] = "-1";
        r.OutputLines.Add($"Cache-Control این پاسخ: {Response.Headers["Cache-Control"]}");
        r.OutputLines.Add("کجا لازم است؟ هر پاسخ با داده شخصی/مالی (پروفایل، فاکتور — صفحه Profile برنامه همین هدرها را باید بگیرد).");
        r.OutputLines.Add("بند مرتبط V14.2.5: کش فقط برای پاسخ‌های با Content-Type مورد انتظار.");
        return View("../Shared/DemoResult", r);
    }

    // ─────────────────────────────────────────────────────────────
    // دموی متنی: طبقه‌بندی داده — بند V14.1.1
    // ─────────────────────────────────────────────────────────────
    public IActionResult DataClassification() =>
        View("../Shared/DemoResult", TextDemo("v14-class", "V14.1.1",
            "همه داده‌های حساس شناسایی و به سطوح محافظت طبقه‌بندی شوند (داده فقط-انکدشده هم حساب می‌شود).", "V14",
            "نمونه طبقه‌بندی برای همین برنامه:\n\n" +
            "سطح بحرانی: کلید امضا (JWT/DataProtection)، رمز دیتابیس → Vault + رمزنگاری + لاگ ممنوع\n" +
            "سطح محرمانه: گذرواژه (فقط هش!)، شماره کارت → رمزنگاری در سکون (دموی بالا)\n" +
            "سطح داخلی: ایمیل، نام کامل، مبلغ فاکتور → کنترل دسترسی V8\n" +
            "سطح عمومی: نام محصول\n\n" +
            "نکته مهم بند: «داده انکدشده» (Base64) هم حساس است — انکد کردن = رمزنگاری نیست؛ دموی V14 را ببینید.\n\n" +
            "این الزام «مستندسازی/فرآیندی» است و در یک اپ آموزشی اجرای رانتایم ندارد؛ طبق خواسته شما متنی ارائه شد."));

    // ─────────────────────────────────────────────────────────────
    // دموی متنی: داده حساس در URL — بند V14.2.1
    // ─────────────────────────────────────────────────────────────
    public IActionResult SensitiveInUrl() =>
        View("../Shared/DemoResult", TextDemo("v14-url", "V14.2.1",
            "داده حساس فقط در بدنه/هدر پیام ارسال شود؛ URL و Query String ممنوع است.", "V14",
            "چرا URL خطرناک است؟\n" +
            "• در History مرورگر، لاگ سرور، لاگ proxy و Referrer می‌ماند\n" +
            "• در اشتراک‌گذاری لینک، رمز همراه لینک ردیف می‌شود!\n\n" +
            "مثال مهاجم‌پسند:\n" +
            "❌ GET /reset?email=ali@example.com&token=ABC123   → توکن در لاگ Nginx!\n" +
            "✅ POST /reset  (body: { token: \"ABC123\" })\n\n" +
            "الگوی درست در MVC:\n" +
            "[HttpPost][ValidateAntiForgeryToken]\n" +
            "public IActionResult Reset(string token) { ... }  // از فرم، نه route\n\n" +
            "این برنامه فرم‌های حساس (لاگین/آپلود) را با POST پیاده کرده و هیچ توکنی در URL ندارد — " +
            "دموی «اجرایی» جدا لازم نداشت چون سیاست جاری برنامه خودش انطباق است."));

    // ─────────────────────────────────────────────────────────────
    // دموی متنی: پاک‌سازی پس از نشست — بند V14.3.1
    // ─────────────────────────────────────────────────────────────
    public IActionResult ClearSiteData() =>
        View("../Shared/DemoResult", TextDemo("v14-clear", "V14.3.1",
            "پس از پایان نشست، داده احراز هویت سمت کلاینت (DOM، localStorage و...) پاک شود.", "V14",
            "دو مکمل همزمان لازم است:\n\n" +
            "۱) سرور: هدر Clear-Site-Data هنگام خروج:\n" +
            "   Response.Headers[\"Clear-Site-Data\"] = \"\\\"cache\\\", \\\"cookies\\\", \\\"storage\\\"\";\n" +
            "   (مرورگر کش/کوکی/web storage را پاک می‌کند)\n\n" +
            "۲) کلاینت: اسکریپت خروج که localStorage/sessionStorage را صریحاً clear() کند — " +
            "چون اگر سرور در لحظه خروج در دسترس نباشد، هدر هرگز نمی‌رسد (بند صراحتاً این حالت را می‌خواهد).\n\n" +
            "در همین برنامه: Logout سشن سرور را Clear/Abandon می‌کند و کوکی‌ها Delete می‌شوند (بند V7.4.1)؛ " +
            "چون SPA نداریم داده‌ای در localStorage نگه نمی‌داریم — که خودش انطباق با V14.3.3 است (داده حساس در web storage ممنوع)."));
}
