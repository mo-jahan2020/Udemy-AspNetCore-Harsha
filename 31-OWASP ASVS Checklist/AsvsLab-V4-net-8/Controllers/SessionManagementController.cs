using AsvsLab.Models;
using Microsoft.AspNetCore.Mvc;

namespace AsvsLab.Controllers;

// ============================================================================
// ⏳ بخش دوم: مدیریت نشست (Session Management)
// فصل V3 استاندارد OWASP ASVS 4.0.3
// ============================================================================
public class SessionManagementController : Controller
{
    // نام کوکی نشست — در Program.cs تعریف شده (پیشوند __Host- مطابق V3.4.4)
    private const string SessionCookieName = "__Host-AsvsLab.Session";

    // ------------------------------------------------------------------------
    // فهرست دموهای این بخش (صفحه اول کنترلر)
    // ------------------------------------------------------------------------
    private static readonly SectionCatalog Catalog = new()
    {
        Title = "بخش ۲: مدیریت نشست (Session Management)",
        ChapterRef = "فصل V3 — OWASP ASVS 4.0.3",
        Demos =
        {
            new() { Action = "CookieFlags", Title = "پرچم‌های امنیتی کوکی (Secure/HttpOnly/SameSite)", AsvsRef = "V3.4.1 — V3.4.4" },
            new() { Action = "SessionTimeout", Title = "انقضای خودکار نشست پس از بی‌فعالیتی", AsvsRef = "V3.3.2" },
            new() { Action = "LogoutInvalidation", Title = "ابطال کامل نشست هنگام خروج", AsvsRef = "V3.3.1" },
            new() { Action = "SessionFixation", Title = "توکن جدید هنگام ورود (ضد Session Fixation)", AsvsRef = "V3.2.1 — V3.1.1" },
        }
    };

    [HttpGet]
    public IActionResult Index() => View("../Shared/SectionIndex", Catalog);

    // ------------------------------------------------------------------------
    // 🎯 دمو ۱: پرچم‌های امنیتی کوکی نشست
    //
    // ⚙️ بندهای استاندارد OWASP ASVS 4.0.3:
    //   V3.4.1 → کوکی نشست باید ویژگی Secure داشته باشد (فقط روی HTTPS ارسال شود)
    //   V3.4.2 → کوکی نشست باید ویژگی HttpOnly داشته باشد (JS نتواند بخواند)
    //   V3.4.3 → کوکی نشست باید ویژگی SameSite داشته باشد (ضد CSRF)
    //   V3.4.4 → استفاده از پیشوند __Host- برای محدودیت دامنه/مسیر
    // ------------------------------------------------------------------------
    [HttpGet]
    public IActionResult CookieFlags()
    {
        var result = new DemoResult
        {
            Title = "پرچم‌های امنیتی کوکی نشست (Secure / HttpOnly / SameSite)",
            AsvsRef = "V3.4.1 — V3.4.2 — V3.4.3 — V3.4.4",
            AsvsSummary = "کوکی نشست باید Secure، HttpOnly و SameSite باشد و ترجیحاً از پیشوند __Host- استفاده کند.",
            Explanation = "بدون HttpOnly، هر XSS ساده‌ای می‌تواند با document.cookie نشست شما را بدزدد. بدون SameSite، سایت مخرب می‌تواند در حمله CSRF کوکی شما را به همراه درخواست خود بفرستد. بدون Secure، کوکی روی HTTP ساده و در معرض شنود ارسال می‌شود. در این دمو یک کوکی با همه این پرچم‌ها ساخته و هدر Set-Cookie واقعی آن نمایش داده می‌شود. پیکربندی همین برنامه در Program.cs هم دقیقاً همین پرچم‌ها را برای کوکی نشست اعمال کرده است.",
            CodeSnippet = """
                // در Program.cs (پیکربندی واقعی نشست این پروژه):
                builder.Services.AddSession(options => {
                    options.Cookie.HttpOnly   = true;                      // V3.4.2
                    options.Cookie.SameSite   = SameSiteMode.Lax;          // V3.4.3
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // V3.4.1
                    options.Cookie.Name       = "__Host-AsvsLab.Session";  // V3.4.4
                });

                // ساخت کوکی دمو با همه پرچم‌ها:
                Response.Cookies.Append("DemoSecureCookie", "value", new CookieOptions {
                    Secure = true, HttpOnly = true,
                    SameSite = SameSiteMode.Strict, IsEssential = true
                });
                """,
            BackController = "SessionManagement"
        };

        // ------------------------- اجرای واقعی دمو -------------------------
        // ساخت یک کوکی نمونه با همه پرچم‌های امنیتی
        Response.Cookies.Append("AsvsLabDemoCookie", "سـر-آزمایشی-123", new CookieOptions
        {
            Secure = true,                    // V3.4.1
            HttpOnly = true,                  // V3.4.2
            SameSite = SameSiteMode.Strict,   // V3.4.3
            Path = "/",
            IsEssential = true
        });

        // خواندن هدر Set-Cookie واقعی که همین الان برای مرورگر ارسال شد
        var setCookieHeader = Response.Headers["Set-Cookie"].ToString();

        result.OutputLines.Add("هدر Set-Cookie واقعی تولیدشده توسط این درخواست:");
        result.OutputLines.Add(setCookieHeader);
        result.OutputLines.Add("");
        result.OutputLines.Add("تفسیر پرچم‌ها:");
        result.OutputLines.Add("  HttpOnly=True   → جاوااسکریپت (مثلاً در حمله XSS) نمی‌تواند کوکی را بخواند");
        result.OutputLines.Add("  Secure=True     → کوکی فقط روی اتصال HTTPS ارسال می‌شود");
        result.OutputLines.Add("  SameSite=Strict → کوکی در درخواست‌های cross-site از سایت‌های دیگر فرستاده نمی‌شود");
        result.OutputLines.Add("");
        result.OutputLines.Add($"کوکی نشستِ خود برنامه هم نام «{SessionCookieName}» دارد:");
        result.OutputLines.Add("  • پیشوند __Host- باعث می‌شود مرورگر کوکی را فقط برای همین دامنه و مسیر ریشه نگه دارد (V3.4.4)");
        result.OutputLines.Add("  • نکته: در دموی محلی روی http، برخی مرورگرها کوکی __Host- یا Secure را نادیده می‌گیرند؛");
        result.OutputLines.Add("    در انتشار واقعی روی HTTPS این پرچم‌ها کامل اعمال می‌شوند.");

        return View("../Shared/DemoResult", result);
    }

    // ------------------------------------------------------------------------
    // 🎯 دمو ۲: انقضای نشست (Timeout)
    //
    // ⚙️ بند استاندارد OWASP ASVS 4.0.3:
    //   V3.3.2 → اگر کاربر اجازه بماندن لاگین دارد، احراز هویت باید به‌صورت
    //            دوره‌ای (فعال یا پس از بی‌فعالیتی) تکرار شود
    // ------------------------------------------------------------------------
    [HttpGet]
    public IActionResult SessionTimeout()
    {
        var result = new DemoResult
        {
            Title = "انقضای خودکار نشست پس از بی‌فعالیتی",
            AsvsRef = "V3.3.2",
            AsvsSummary = "نشست باید پس از دوره بی‌فعالیتی مشخص منقضی شود (سطح L2: حداکثر ۱۲ ساعت یا ۳۰ دقیقه بی‌فعالیتی).",
            Explanation = "اگر کاربری لپ‌تاپش را در کافه رها کند، نشستِ باز او نباید برای همیشه معتبر بماند. در این برنامه IdleTimeout روی ۱۵ دقیقه تنظیم شده است؛ یعنی اگر کاربر ۱۵ دقیقه هیچ درخواستی نفرستد، نشست و همه داده‌هایش به‌طور خودکار پاک می‌شود و درخواست بعدیِ او با نشست خالی مواجه می‌گردد. توجه کنید که IdleTimeout در ASP.NET Core هر بار با هر درخواستِ فعال، تازه (Sliding) می‌شود.",
            CodeSnippet = """
                // در Program.cs (پیکربندی واقعی این پروژه):
                builder.Services.AddSession(options => {
                    // انقضای نشست پس از ۱۵ دقیقه بی‌فعالیتی (بند V3.3.2)
                    options.IdleTimeout = TimeSpan.FromMinutes(15);
                });
                """,
            BackController = "SessionManagement"
        };

        // ------------------------- اجرای واقعی دمو -------------------------
        // نوشتن یک مقدار در نشست تا وضعیت فعلی نشست نمایش داده شود
        HttpContext.Session.SetString("LastDemoRun", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        result.OutputLines.Add($"پیکربندی فعلی برنامه: IdleTimeout = 15 دقیقه (V3.3.2)");
        result.OutputLines.Add("");
        result.OutputLines.Add($"کد نشست (Session ID) فعلی: {HttpContext.Session.Id[..12]}...");
        result.OutputLines.Add($"مقدار ذخیره‌شده در نشست (LastDemoRun): {HttpContext.Session.GetString("LastDemoRun")}");
        result.OutputLines.Add("");
        result.OutputLines.Add("تست عملی: این صفحه را باز بگذارید، ۱۵ دقیقه کاری نکنید، سپس روی دکمه‌ای بزنید؛");
        result.OutputLines.Add("می‌بینید که Session Id عوض شده و مقدار LastDemoRun پاک شده است.");
        result.OutputLines.Add("");
        result.OutputLines.Add("در سطح L2 استاندارد، سقف مجاز: نشست ۱۲ ساعته یا ۳۰ دقیقه بی‌فعالیتی است.");

        return View("../Shared/DemoResult", result);
    }

    // ------------------------------------------------------------------------
    // 🎯 دمو ۳: ابطال واقعی نشست هنگام خروج (Logout)
    //
    // ⚙️ بند استاندارد OWASP ASVS 4.0.3:
    //   V3.3.1 → خروج و انقضا باید توکن نشست را باطل کنند؛ دکمه Back مرورگر
    //            نباید نشست احرازشده را زنده کند
    // ------------------------------------------------------------------------
    [HttpGet]
    public IActionResult LogoutInvalidation()
    {
        var result = new DemoResult
        {
            Title = "ابطال کامل نشست هنگام خروج",
            AsvsRef = "V3.3.1",
            AsvsSummary = "خروج و انقضا باید توکن نشست را باطل کنند تا دکمه Back یا بازیابی از حافظه، نشست احرازشده را برنگرداند.",
            Explanation = "خیلی از برنامه‌ها فقط کوکی را حذف می‌کنند، اما داده‌های نشست سمت سرور باقی می‌ماند. logout امن دو کار همزمان می‌کند: (۱) پاک‌سازی کامل داده‌های نشست سمت سرور (Session.Clear) و (۲) حذف کوکی نشست از مرورگر. در این دمو ابتدا یک «دسترسی پنل مدیریت» به نشست اضافه می‌کنیم، سپس Logout امن را اجرا و مجدداً همان مقدار را می‌خوانیم؛ می‌بینید که دیگر هیچ داده‌ای در دسترس نیست.",
            CodeSnippet = """
                // Logout امن: پاک‌سازی سمت سرور + حذف کوکی
                HttpContext.Session.Clear();                 // پاک کردن داده‌های نشست سمت سرور
                Response.Cookies.Delete(SessionCookieName);  // حذف کوکی از مرورگر
                HttpContext.Session.Remove("AdminPanelAccess");

                // حالا حتی با دکمه Back هم داده‌های نشست قابل بازیابی نیستند
                """,
            BackController = "SessionManagement"
        };

        // ------------------------- اجرای واقعی دمو -------------------------
        // گام ۱: شبیه‌سازی کاربر واردشده
        HttpContext.Session.SetString("AdminPanelAccess", "ACTIVE");
        var beforeClear = HttpContext.Session.GetString("AdminPanelAccess");

        // گام ۲: اجرای Logout امن
        HttpContext.Session.Clear();
        Response.Cookies.Delete(SessionCookieName);

        // گام ۳: تلاش برای خواندن مجدد داده حساس
        var afterClear = HttpContext.Session.GetString("AdminPanelAccess");

        result.OutputLines.Add("گام ۱ - کاربر وارد شده و در نشست او دسترسی پنل ثبت شده است:");
        result.OutputLines.Add($"   AdminPanelAccess = {beforeClear}");
        result.OutputLines.Add("");
        result.OutputLines.Add("گام ۲ - اجرای Logout امن (Session.Clear + حذف کوکی):");
        result.OutputLines.Add("   ✓ داده‌های نشست سمت سرور پاک شد");
        result.OutputLines.Add($"   ✓ کوکی «{SessionCookieName}» حذف شد");
        result.OutputLines.Add("");
        result.OutputLines.Add("گام ۳ - تلاش برای خواندن مجدد داده حساس:");
        result.OutputLines.Add($"   AdminPanelAccess = {(afterClear ?? "null (هیچ داده‌ای باقی نمانده ✓)")}");
        result.OutputLines.Add("");
        result.OutputLines.Add("✓ نتیجه: حتی اگر مهاجم توکن قبلی را داشته باشد، سمت سرور هیچ داده‌ای به آن متصل نیست.");

        return View("../Shared/DemoResult", result);
    }

    // ------------------------------------------------------------------------
    // 🎯 دمو ۴: مقابله با Session Fixation (تولید توکن جدید هنگام ورود)
    //
    // ⚙️ بندهای استاندارد OWASP ASVS 4.0.3:
    //   V3.2.1 → برنامه باید هنگام احراز هویت کاربر، توکن نشست جدید تولید کند
    //   V3.1.1 → توکن نشست هرگز نباید در پارامترهای URL ظاهر شود
    // ------------------------------------------------------------------------
    [HttpGet]
    public IActionResult SessionFixation()
    {
        var result = new DemoResult
        {
            Title = "تولید توکن نشست جدید هنگام ورود (ضد Session Fixation)",
            AsvsRef = "V3.2.1 — V3.1.1",
            AsvsSummary = "برنامه باید هنگام احراز هویت، توکن نشست جدیدی تولید کند و توکن‌ها هرگز در URL فاش نشوند.",
            Explanation = "در حمله Session Fixation مهاجم کاربر را وادار می‌کند با یک توکنِ نشستِ از پیش تعیین‌شدهِ او وارد شود؛ اگر برنامه پس از ورود همان توکن قدیمی را نگه دارد، مهاجم (که توکن را می‌داند) به نشست کاربر نفوذ می‌کند. راه حل: هنگام لاگین موفق، نشست قبلی را کاملاً خالی و توکن جدید صادر کنید. این دمو توکن نشست قبل و بعد از «ورود» را نشان می‌دهد. همچنین بند V3.1.1: توکن نشست فقط در کوکی امن منتقل می‌شود و هرگز در URL نمی‌آید.",
            CodeSnippet = """
                // هنگام لاگین موفق (ضد Session Fixation):
                HttpContext.Session.Clear();                 // خالی کردن نشست قدیمی مهاجم/کاربر
                Response.Cookies.Delete(SessionCookieName);  // حذف توکن قبلی

                // ثبت اطلاعات کاربر و صدور توکن کاملاً جدید:
                HttpContext.Session.SetString("UserId", user.Id.ToString());
                // ASP.NET Core هنگام اولین دسترسی به Session، خودش توکن جدید می‌سازد
                """,
            BackController = "SessionManagement"
        };

        // ------------------------- اجرای واقعی دمو -------------------------
        var tokenBefore = Request.Cookies[SessionCookieName] ?? "(توکن قبلی وجود ندارد)";

        // شبیه‌سازی «ورود موفق»: پاک‌سازی + صدور توکن جدید
        HttpContext.Session.Clear();
        Response.Cookies.Delete(SessionCookieName);
        HttpContext.Session.SetString("UserId", "42"); // دسترسی مجدد → توکن جدید صادر می‌شود

        result.OutputLines.Add("قبل از ورود (توکن قدیمی که احتمالاً در دست مهاجم بوده):");
        result.OutputLines.Add($"   {tokenBefore[..Math.Min(60, tokenBefore.Length)]}...");
        result.OutputLines.Add("");
        result.OutputLines.Add("اجرای فرآیند لاگین امن: Session.Clear + حذف کوکی + صدور توکن جدید");
        result.OutputLines.Add("");
        result.OutputLines.Add($"توکن نشست جدید (Session Id): {HttpContext.Session.Id[..24]}...");
        result.OutputLines.Add("");
        result.OutputLines.Add("✓ چون توکن کاملاً جدید است، توکن دست‌کاری‌شده مهاجم دیگر به هیچ نشستی وصل نیست.");
        result.OutputLines.Add("✓ بند V3.1.1: در تمام این برنامه، توکن نشست فقط در کوکی منتقل می‌شود و در URL ظاهر نمی‌شود.");

        return View("../Shared/DemoResult", result);
    }
}
