using AsvsLab.Models;
using Microsoft.AspNetCore.Mvc;

namespace AsvsLab.Controllers;

// ============================================================================
// ⚙️ بخش هشتم: پیکربندی امن (Security Configuration)
// فصل V14 استاندارد OWASP ASVS 4.0.3
// ============================================================================
public class ConfigurationController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ConfigurationController> _logger;

    public ConfigurationController(IConfiguration configuration, ILogger<ConfigurationController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    // ------------------------------------------------------------------------
    // فهرست دموهای این بخش (صفحه اول کنترلر)
    // ------------------------------------------------------------------------
    private static readonly SectionCatalog Catalog = new()
    {
        Title = "بخش ۸: پیکربندی امن",
        ChapterRef = "فصل V14 — OWASP ASVS 4.0.3",
        Demos =
        {
            new() { Action = "SecurityHeaders", Title = "هدرهای امنیتی پاسخ‌های HTTP", AsvsRef = "V14.4.1 — V14.4.7" },
            new() { Action = "NoVersionInfo", Title = "عدم افشای نسخه وب‌سرور و فریم‌ورک", AsvsRef = "V14.3.3" },
            new() { Action = "SecretsManagement", Title = "مدیریت امن مقادیر محرمانه (User Secrets)", AsvsRef = "V6.4.1 — V14.1.3" },
            new() { Action = "SriExplanation", Title = "یکپارچگی دارایی‌های CDN با SRI (توضیح متنی)", AsvsRef = "V14.2.3" },
        }
    };

    [HttpGet]
    public IActionResult Index() => View("../Shared/SectionIndex", Catalog);

    // ------------------------------------------------------------------------
    // 🎯 دمو ۱: هدرهای امنیتی پاسخ‌های HTTP
    //
    // ⚙️ بندهای استاندارد OWASP ASVS 4.0.3:
    //   V14.4.1 → هر پاسخ باید هدر Content-Type با charset امن داشته باشد
    //   V14.4.3 → هدر Content-Security-Policy برای کاهش اثر XSS
    //   V14.4.4 → هدر X-Content-Type-Options: nosniff
    //   V14.4.5 → هدر Strict-Transport-Security (HSTS)
    //   V14.4.6 → هدر Referrer-Policy
    //   V14.4.7 → ممنوعیت قرارگیری صفحه در iframe سایت‌های دیگر
    // ------------------------------------------------------------------------
    [HttpGet]
    public IActionResult SecurityHeaders()
    {
        var result = new DemoResult
        {
            Title = "هدرهای امنیتی پاسخ‌های HTTP",
            AsvsRef = "V14.4.1 — V14.4.3 — V14.4.4 — V14.4.5 — V14.4.6 — V14.4.7",
            AsvsSummary = "پاسخ‌های HTTP باید هدرهای امنیتی (CSP، nosniff، HSTS، Referrer-Policy، X-Frame-Options) را داشته باشند.",
            Explanation = "این هدرها «لایه دفاع دوم» مرورگر هستند: CSP حتی اگر XSS رخ دهد اجرای اسکریپت خارجی را می‌بندد؛ nosniff جلوی حدس زدن نوع فایل (اجرای فایل آپلودی به‌عنوان HTML/JS) را می‌گیرد؛ HSTS مرورگر را مجبور می‌کند فقط HTTPS صحبت کند؛ X-Frame-Options جلوی Clickjacking (قرار گرفتن صفحه در iframe نامرئی) را می‌گیرد. این برنامه این هدرها را در میان‌افزار Program.cs به همه پاسخ‌ها اضافه می‌کند — این دمو همان هدرهای واقعیِ همین پاسخ را می‌خواند و نشان می‌دهد.",
            CodeSnippet = """
                // میان‌افزار در Program.cs — به همه پاسخ‌ها اعمال می‌شود:
                app.Use(async (context, next) => {
                    context.Response.Headers["Content-Security-Policy"] =
                        "default-src 'self'; ...";   // V14.4.3
                    context.Response.Headers["X-Content-Type-Options"] = "nosniff"; // V14.4.4
                    context.Response.Headers["Referrer-Policy"] = "no-referrer";   // V14.4.6
                    context.Response.Headers["X-Frame-Options"] = "DENY";          // V14.4.7
                    await next();
                });

                // HSTS فقط در Production (بند V14.4.5):
                if (!app.Environment.IsDevelopment()) app.UseHsts();
                """,
            BackController = "Configuration"
        };

        // ------------------------- اجرای واقعی دمو -------------------------
        // هدرهای واقعیِ همین پاسخ (توسط میان‌افزار Program.cs تنظیم شده‌اند)
        result.OutputLines.Add("هدرهای امنیتیِ واقعیِ همین پاسخ HTTP (توسط میان‌افزار تنظیم شده‌اند):");
        result.OutputLines.Add("");
        // اگر Content-Type هنوز تنظیم نشده باشد (پیش از رندر ویو)، مقدار استاندارد را نشان می‌دهیم
        string contentType = Response.Headers["Content-Type"].ToString();
        if (string.IsNullOrEmpty(contentType)) contentType = "text/html; charset=utf-8";
        result.OutputLines.Add($"   Content-Type: {contentType}   (V14.4.1)");
        result.OutputLines.Add($"   Content-Security-Policy: {Response.Headers["Content-Security-Policy"]}   (V14.4.3)");
        result.OutputLines.Add($"   X-Content-Type-Options: {Response.Headers["X-Content-Type-Options"]}   (V14.4.4)");
        result.OutputLines.Add($"   Referrer-Policy: {Response.Headers["Referrer-Policy"]}   (V14.4.6)");
        result.OutputLines.Add($"   X-Frame-Options: {Response.Headers["X-Frame-Options"]}   (V14.4.7)");
        result.OutputLines.Add("");
        result.OutputLines.Add("برای مشاهده: F12 → Network → این درخواست → Response Headers");
        result.OutputLines.Add("");
        result.OutputLines.Add("نکته HSTS (بند V14.4.5): هدر Strict-Transport-Security فقط در Production و روی");
        result.OutputLines.Add("HTTPS معنا دارد؛ در Program.cs با app.UseHsts() فعال شده است.");

        return View("../Shared/DemoResult", result);
    }

    // ------------------------------------------------------------------------
    // 🎯 دمو ۲: عدم افشای نسخه و اطلاعات سرور
    //
    // ⚙️ بند استاندارد OWASP ASVS 4.0.3:
    //   V14.3.3 → هدرهای HTTP نباید نسخه دقیق اجزای سیستم را فاش کنند
    // ------------------------------------------------------------------------
    [HttpGet]
    public IActionResult NoVersionInfo()
    {
        var result = new DemoResult
        {
            Title = "عدم افشای نسخه وب‌سرور و فریم‌ورک",
            AsvsRef = "V14.3.3",
            AsvsSummary = "پاسخ‌های HTTP نباید اطلاعات نسخه دقیق اجزای سیستم (وب‌سرور، فریم‌ورک، افزونه‌ها) را فاش کنند.",
            Explanation = "اگر پاسخ‌ها هدر Server: Kestrel/12.0.1 یا X-AspNet-Version داشته باشند، مهاجم فقط با یک درخواست، نسخه دقیق فناوری شما را می‌فهمد و exploitهای مربوط به همان نسخه را امتحان می‌کند. در این برنامه با AddServerHeader=false هدر Server کلاً حذف شده است. این دمو هدرهای واقعی پاسخ جاری را بررسی می‌کند.",
            CodeSnippet = """
                // در Program.cs (بند V14.3.3):
                builder.WebHost.UseKestrel(options => {
                    options.AddServerHeader = false; // حذف هدر Server
                });

                // برای IIS در web.config:
                // <requestFiltering removeServerHeader="true" />
                """,
            BackController = "Configuration"
        };

        // ------------------------- اجرای واقعی دمو -------------------------
        result.OutputLines.Add("بررسی هدرهای واقعیِ همین پاسخ:");
        result.OutputLines.Add("");
        var serverHeader = Response.Headers.ContainsKey("Server") ? Response.Headers["Server"].ToString() : "";
        result.OutputLines.Add($"   هدر Server: {(string.IsNullOrEmpty(serverHeader) ? "❌ وجود ندارد ✓ (به‌درستی حذف شده)" : $"⚠️ {serverHeader}")}");
        var aspNetHeader = Response.Headers.ContainsKey("X-AspNet-Version") ? Response.Headers["X-AspNet-Version"].ToString() : "";
        result.OutputLines.Add($"   هدر X-AspNet-Version: {(string.IsNullOrEmpty(aspNetHeader) ? "❌ وجود ندارد ✓" : $"⚠️ {aspNetHeader}")}");
        result.OutputLines.Add("");
        result.OutputLines.Add("✓ مهاجم نمی‌تواند با یک درخواست ساده، نسخه سرور و فریم‌ورک را شناسایی کند.");
        result.OutputLines.Add("  (در انتشار واقعی، حذف هدر Kestrel Validation و توکن‌های اضافی را هم چک کنید)");

        return View("../Shared/DemoResult", result);
    }

    // ------------------------------------------------------------------------
    // 🎯 دمو ۳: مدیریت امن مقادیر محرمانه (Secrets Management)
    //
    // ⚙️ بند استاندارد OWASP ASVS 4.0.3:
    //   V6.4.1 → برای ایجاد، ذخیره و نابودی مقادیر محرمانه از راهکار امن استفاده شود
    //   V14.1.3 → پیکربندی سرور مطابق توصیه‌های امنیتی سخت‌سازی (Hardening) شود
    // ------------------------------------------------------------------------
    [HttpGet]
    public IActionResult SecretsManagement()
    {
        var result = new DemoResult
        {
            Title = "مدیریت امن مقادیر محرمانه (خارج کردن Secret از کد)",
            AsvsRef = "V6.4.1 — V14.1.3",
            AsvsSummary = "مقادیر محرمانه (کلید API، رمز دیتابیس) نباید در سورس‌کد یا appsettings.json کامیت شوند؛ از User Secrets / متغیر محیطی / Key Vault استفاده کنید.",
            Explanation = "قرار دادن کلید API در appsettings.json یعنی آپلود آن در مخزن Git — و حتی اگر بعداً حذفش کنید، در تاریخچه Git باقی می‌ماند! روش صحیح در توسعه: User Secrets (ذخیره خارج از پوشه پروژه) و در Production: متغیر محیطی یا Key Vault. این دمو می‌خواند که برنامه چگونه مقدار را از هر منبعی که پیکربندی شده می‌خواند، بدون اینکه مقدار واقعی در کد/مخزن باشد.",
            CodeSnippet = """
                # یک بار در پوشه پروژه: فعال‌سازی User Secrets
                dotnet user-secrets init

                # ذخیره مقدار محرمانه (خارج از سورس‌کد و مخزن Git)
                dotnet user-secrets set "DemoSettings:FakeApiKey" "REAL-KEY-HERE"

                # در کد، بدون تغییر، همان IConfiguration خوانده می‌شود:
                var key = _configuration["DemoSettings:FakeApiKey"];

                # در Production: متغیر محیطی
                #   export DemoSettings__FakeApiKey="REAL-KEY-HERE"
                # یا Key Vault / HashiCorp Vault (بند V6.4.1)
                """,
            BackController = "Configuration"
        };

        // ------------------------- اجرای واقعی دمو -------------------------
        string? apiKey = _configuration["DemoSettings:FakeApiKey"];

        result.OutputLines.Add("وضعیت فعلی این برنامه:");
        result.OutputLines.Add($"   مقدار DemoSettings:FakeApiKey در پیکربندی فعال: {(string.IsNullOrEmpty(apiKey) ? "تعریف نشده" : "پیدا شد (نمایش نمی‌کنیم!)")}");
        if (!string.IsNullOrEmpty(apiKey))
            result.OutputLines.Add($"   ماسک‌شده: {apiKey[..4]}**** (فقط برای اثبات وجود — هرگز مقدار کامل لاگ/نمایش نکنید)");
        result.OutputLines.Add("");
        result.OutputLines.Add("مراحل صحیح در همین پروژه (تست کنید):");
        result.OutputLines.Add("   1) در پوشه پروژه اجرا کنید:  dotnet user-secrets init");
        result.OutputLines.Add("   2)                          dotnet user-secrets set \"DemoSettings:FakeApiKey\" \"کلید-واقعی-من\"");
        result.OutputLines.Add("   3) برنامه را دوباره اجرا کنید — بدون هیچ تغییر کدی، مقدار از User Secrets خوانده می‌شود");
        result.OutputLines.Add("");
        result.OutputLines.Add("⚠️ فایل appsettings.json این پروژه عمداً یک مقدار نمادین دارد تا خطر کامیت Secret واقعی را نشان دهد.");
        result.OutputLines.Add("   (فایل secrets.json خارج از پوشه پروژه، در پروفایل کاربر نگهداری می‌شود و به Git نمی‌رود)");

        return View("../Shared/DemoResult", result);
    }

    // ------------------------------------------------------------------------
    // 🎯 دمو ۴ (متن‌محور): یکپارچگی دارایی‌های سمت کلاینت (SRI) — بخش غیرقابل اجرای واقعی
    //
    // ⚙️ بند استاندارد OWASP ASVS 4.0.3:
    //   V14.2.3 → اگر دارایی‌ها (JS/CSS/فونت) از CDN بیرونی لود می‌شوند، باید
    //              از Subresource Integrity (SRI) استفاده شود
    //
    // ℹ️ این دمو به‌صورت «متنی» توضیح داده می‌شود؛ چون اجرای واقعی SRI نیازمند
    //    لود کردن فایل از CDN خارجی است که با رویکرد آفلاینِ این پروژه منافات دارد.
    // ------------------------------------------------------------------------
    [HttpGet]
    public IActionResult SriExplanation()
    {
        var result = new DemoResult
        {
            Title = "یکپارچگی دارایی‌های CDN با SRI (توضیح متنی)",
            AsvsRef = "V14.2.3",
            AsvsSummary = "اگر فایل‌های JS/CSS از CDN لود می‌شوند، باید با Subresource Integrity (SRI) اعتبارشان بررسی شود.",
            Explanation = "این بند را نمی‌توان در همین برنامه محلی «اجرا» کرد، چون: این پروژه عمداً Bootstrap و فونت‌ها را به‌صورت آفلاین و محلی در wwwroot نگه می‌دارد — و فایل محلی هیچ CDN و SRI لازم ندارد! (به عبارتی، استفاده از فایل محلی، خودِ ریسک این بند را از اساس حذف می‌کند.) اما اگر در پروژه واقعی‌تان فایلی از CDN می‌خوانید، حتماً hash integrity آن را در تگ script بگذارید تا اگر CDN هک شد و فایل جاسازی‌شده سرو شد، مرورگر آن را اجرا نکند.",
            CodeSnippet = """
                <!-- ❌ ریسک: اگر CDN هک شود، کد مهاجم در صفحه شما اجرا می‌شود -->
                <script src="https://cdn.example.com/bootstrap.min.js"></script>

                <!-- ✓ با SRI (بند V14.2.3): هش فایل تعبیه شده است -->
                <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"
                        integrity="sha384-HwwvtgBNo3bZJJLYd8oVXjrBZt8cqVSpeBNS5n7C8WRIn4DHDNpfG7vpmPyRStDJ"
                        crossorigin="anonymous"></script>

                <!-- ✓ رویکرد این پروژه: فایل محلی آفلاین → نیاز به SRI ندارد -->
                <script src="~/lib/bootstrap/dist/js/bootstrap.bundle.min.js"></script>
                """,
            OutputLines =
            {
                "چرا این دمو فقط متنی است؟",
                "  • پروژه به‌صورت آفلاین طراحی شده و فایل‌ها را از CDN لود نمی‌کند.",
                "  • استفاده از فایل محلی = حذف کامل ریسک دستکاری CDN (خودِ بند V14.2.3 نگران CDN است).",
                "",
                "اگر در پروژه خودتان از CDN استفاده می‌کنید:",
                "  • هش SRI را از https://www.srihash.org بگیرید و در اتریبیوت integrity بگذارید.",
                "  • همچنین اتریبیوت crossorigin=\"anonymous\" را فراموش نکنید.",
                "",
                "سایر بندهای مرتبط که «اجرا» نیستند و توصیه متن‌محور دارند:",
                "  • V14.1.1 → فرایند Build/نشر باید CI/CD امن و تکرارپذیر باشد (Azure DevOps/GitHub Actions).",
                "  • V14.2.1 → به‌روز بودن وابسته‌ها با dotnet list package --vulnerable چک می‌شود.",
                "  • V14.2.5 → نگهداری SBOM (فهرست مواد نرم‌افزاری) برای همه کتابخانه‌های شخص ثالث."
            },
            BackController = "Configuration"
        };

        return View("../Shared/DemoResult", result);
    }
}
