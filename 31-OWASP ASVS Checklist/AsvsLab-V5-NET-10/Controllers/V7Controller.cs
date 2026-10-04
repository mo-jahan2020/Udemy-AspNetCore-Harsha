using AsvsLab.Services;
using Microsoft.AspNetCore.Mvc;

namespace AsvsLab.Controllers;

// ═══════════════════════════════════════════════════════════════════
//  فصل V7 — مدیریت نشست (Session Management)
//  نکته 5.0: پرچم‌های کوکی به فصل V3 منتقل شده‌اند؛ اینجا تمرکز روی
//  زمان‌بندی، تمدید توکن و ابطال سشن است
// ═══════════════════════════════════════════════════════════════════
public class V7Controller : DemoBaseController
{
    public V7Controller(AsvsRefService asvs, ProgressService progress) : base(asvs, progress) { }

    public IActionResult Index() =>
        SectionIndex(7, "مدیریت نشست: وقفه بی‌فعالیتی، تمدید توکن پس از ورود (ضد Session Fixation) و ابطال کامل در خروج.");

    // ─────────────────────────────────────────────────────────────
    // دموی ۱: وقفه بی‌فعالیتی — بندهای V7.1.1 و V7.3.1
    // ─────────────────────────────────────────────────────────────
    public IActionResult SessionTimeout()
    {
        var r = New("v7-timeout", "V7.1.1,V7.3.1",
            "وقفه بی‌فعالیتی و طول عمر مطلق سشن باید مستند و مبتنی بر ریسک باشد و واقعاً اعمال شود.", "V7");
        r.VulnerableCode = """
            // ❌ ناامن: سشن بی‌پایان (پیش‌فرض‌های قدیمی)
            // بدون IdleTimeout → کوکی سرقت‌شده تا همیشه کار می‌کند
            """;
        r.SecureCode = """
            // ✅ امن (در Program.cs این برنامه فعال است)
            services.AddSession(o => {
                o.IdleTimeout = TimeSpan.FromMinutes(15); // وقفه بی‌فعالیتی
                o.Cookie.MaxAge = TimeSpan.FromHours(8);  // طول عمر مطلق
            });
            """;
        // اجرای واقعی: دستکاری سشن + نمایش شناسه فعلی
        HttpContext.Session.SetString("demo.visit", DateTime.Now.ToString("HH:mm:ss"));
        var last = HttpContext.Session.GetString("demo.visit");
        r.OutputLines.Add($"سشن فعلی شناسه: {HttpContext.Session.Id[..16]}…");
        r.OutputLines.Add($"آخرین دسترسی ثبت‌شده در سشن: {last}");
        r.OutputLines.Add("پیکربندی این برنامه: IdleTimeout = ۱۵ دقیقه؛ بعد از آن سشن سمت سرور باطل می‌شود.");
        r.OutputLines.Add("آزمایش: ۱۵ دقیقه کاری نکنید و صفحه را رفرش کنید — داده سشن خالی است.");
        return View("../Shared/DemoResult", r);
    }

    // ─────────────────────────────────────────────────────────────
    // دموی ۲: تمدید توکن سشن — بند V7.2.4 (ضد Session Fixation)
    // ─────────────────────────────────────────────────────────────
    public IActionResult SessionRenewal()
    {
        var r = New("v7-renew", "V7.2.4",
            "هنگام احراز هویت باید توکن سشن جدید تولید و توکن فعلی خاتمه یابد (ضد تثبیت نشست).", "V7");
        r.VulnerableCode = """
            // ❌ ناامن: ورود بدون تمدید توکن — حمله Session Fixation ممکن است
            // مهاجم کوکی از پیش ساخته را به قربانی می‌دهد؛ پس از ورود قربانی،
            // مهاجم همان کوکی را دارد و وارد حساب قربانی می‌شود!
            await SignInAsync(user); // سشن قدیمی همچنان معتبر
            """;
        r.SecureCode = """
            // ✅ امن: پاک‌سازی و تولید سشن تازه قبل از ورود
            HttpContext.Session.Clear();  // این برنامه همین کار را در Account/Login می‌کند
            await HttpContext.SignInAsync(user);
            """;
        var before = HttpContext.Session.Id;
        HttpContext.Session.Clear();
        HttpContext.Session.SetString("renewed", "1");
        var after = HttpContext.Session.Id;
        r.OutputLines.Add($"شناسه سشن قبل از تمدید: {before[..16]}…");
        r.OutputLines.Add($"شناسه سشن بعد از Clear:  {after[..16]}…");
        r.OutputLines.Add($"توکن تمدید شد؟ {(before != after ? "بله ✅ — توکن جدید صادر شد" : "خیر ❌")}");
        r.OutputLines.Add("در صفحه ورود واقعی برنامه، این عمل قبل از SignInAsync انجام می‌شود.");
        return View("../Shared/DemoResult", r);
    }

    // ─────────────────────────────────────────────────────────────
    // دموی ۳: ابطال سشن در خروج — بند V7.4.1
    // ─────────────────────────────────────────────────────────────
    public IActionResult LogoutInvalidation()
    {
        var r = New("v7-logout", "V7.4.1",
            "پس از خروج یا انقضا، استفاده مجدد از سشن باید غیرممکن شود.", "V7");
        r.VulnerableCode = """
            // ❌ ناامن: «خروج» فقط ظاهری — فقط ریدایرکت!
            return Redirect("/"); // کوکی و داده سشن برجای می‌ماند
            """;
        r.SecureCode = """
            // ✅ امن (دقیقاً در Account/Logout این برنامه):
            HttpContext.Session.Clear();      // داده سشن سرور پاک شد
            await HttpContext.SignOutAsync(); // کوکی احراز هویت باطل شد
            Response.Cookies.Delete("AsvsLab.Auth");
            """;
        HttpContext.Session.SetString("cart", "دفترچه خطرناک باقی‌مانده");
        var cartBefore = HttpContext.Session.GetString("cart");
        HttpContext.Session.Clear();
        var cartAfter = HttpContext.Session.GetString("cart");
        r.OutputLines.Add($"داده سشن قبل از خروج: {cartBefore ?? "—"}");
        r.OutputLines.Add($"داده سشن بعد از Clear: {cartAfter ?? "پاک شد ✅"}");
        r.OutputLines.Add("برای تجربه واقعی: از منو «خروج» را بزنید و بعد Back/رفرش کنید.");
        return View("../Shared/DemoResult", r);
    }

    // ─────────────────────────────────────────────────────────────
    // دموی متنی: اعتبارسنجی سشن در بک‌اند — بند V7.2.1
    // ─────────────────────────────────────────────────────────────
    public IActionResult BackendValidation() =>
        View("../Shared/DemoResult", TextDemo("v7-backend", "V7.2.1",
            "اعتبارسنجی توکن نشست باید همیشه در سرویس مورد اعتماد بک‌اند انجام شود.", "V7",
            "سناریوی خطر: اگر اپلیکیشن «بپذیرد» که کوکی سشن معتبر است چون ساختارش درست به نظر می‌رسد " +
            "(مثل JWT بدون بررسی امضا در سمت سرور)، مهاجم می‌تواند توکن جعلی بسازد.\n\n" +
            "در ASP.NET Core سشن در سرور (حافظه/Redis) نگهداری می‌شود و کوکی فقط یک شناسه تصادفی ۱۲۸ بیتی است؛ " +
            "بنابراین جعل شناسه عملاً غیرممکن است — این «سشن سمت سرور» همان الگوی مورد نظر ASVS است.\n\n" +
            "برای توکن‌های خودبسنده (JWT) این الزام یعنی: امضا + exp + iss همیشه در سرور بررسی شوند؛ " +
            "دموی عملی آن در فصل V9 ارائه شده است.\n\n" +
            "اجرای مستقل این دمو نیازمند زیرساخت جداگانه است؛ بنابراین توضیح متنی + ارجاع به معماری فعلی ارائه شد."));
}
