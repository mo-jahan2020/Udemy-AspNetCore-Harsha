using AsvsLab.Services;
using Microsoft.AspNetCore.Mvc;

namespace AsvsLab.Controllers;

// ═══════════════════════════════════════════════════════════════════
//  فصل V13 — پیکربندی (Configuration)
// ═══════════════════════════════════════════════════════════════════
public class V13Controller : DemoBaseController
{
    private readonly IWebHostEnvironment _env;
    public V13Controller(AsvsRefService asvs, ProgressService progress, IWebHostEnvironment env)
        : base(asvs, progress) => _env = env;

    public IActionResult Index() =>
        SectionIndex(13, "پیکربندی امن: حذف هدر Server، خاموشی حالت دیباگ، مدیریت اسرار (Vault) و چرخش آنها.");

    // ─────────────────────────────────────────────────────────────
    // دموی ۱: حذف هدر Server — بند V13.4.2 (در 4.0.3 با شماره V14.3.3 بود)
    // ─────────────────────────────────────────────────────────────
    public IActionResult ServerHeader()
    {
        var r = New("v13-server", "V13.4.2",
            "اجزا نباید اطلاعات داخلی (نسخه وب‌سرور، فریم‌ورک) افشا کنند؛ اینجا هدر Server حذف شده است.", "V13");
        r.VulnerableCode = """
            // ❌ پیش‌فرض: پاسخ‌ها این هدر را دارند
            Server: Kestrel
            // مهاجم از نسخه سرور برای یافتن exploit مناسب استفاده می‌کند
            """;
        r.SecureCode = """
            // ✅ در Program.cs همین برنامه:
            builder.WebHost.ConfigureKestrel(o => o.AddServerHeader = false);
            // هدرهای اضافی مثل X-Powered-By هم هرگز ست نمی‌شوند
            """;
        var server = Response.Headers.ContainsKey("Server") ? Response.Headers["Server"].ToString() : "(ندارد)";
        r.OutputLines.Add($"هدر Server این پاسخ: {server}");
        r.OutputLines.Add($"محیط اجرا: {_env.EnvironmentName} (اطلاعات محیط هم نباید در پاسخ باشد)");
        r.OutputLines.Add("آزمایش: با curl -I آدرس برنامه را بزنید — هدر Server وجود ندارد.");
        return View("../Shared/DemoResult", r);
    }

    // ─────────────────────────────────────────────────────────────
    // دموی ۲: حالت دیباگ خاموش — بند V13.4.2
    // ─────────────────────────────────────────────────────────────
    public IActionResult DebugMode()
    {
        var r = New("v13-debug", "V13.4.2",
            "حالت دیباگ همه اجزا در محیط عملیاتی باید غیرفعال باشد (نشت اطلاعات + قابلیت‌های ویژه دیباگ).", "V13");
        r.VulnerableCode = """
            // ❌ ناامن در تولید: صفحه خطای کامل با Stack Trace
            app.UseDeveloperExceptionPage();
            // مسیر‌های source، متغیرها و کوئری‌ها را به مهاجم نشان می‌دهد!
            """;
        r.SecureCode = """
            // ✅ امن (در Program.cs این برنامه):
            app.UseExceptionHandler("/Home/Error"); // پیام عمومی
            // و در csproj/appsettings خطای کامل فقط برای Development
            """;
        var isDev = _env.IsDevelopment();
        r.OutputLines.Add($"محیط فعلی: {_env.EnvironmentName} — DeveloperExceptionPage: {(isDev ? "روشن (فقط برای توسعه)" : "خاموش ✅")}");
        r.OutputLines.Add("حتی در محیط توسعه هم صفحه خطای کامل «فقط» به توسعه‌دهنده با دسترسی سرور می‌رسد.");
        r.OutputLines.Add("در تولید واقعی: خطای کامل فقط در «لاگ» ثبت می‌شود (فصل V16)، نه در پاسخ HTTP.");
        return View("../Shared/DemoResult", r);
    }

    // ─────────────────────────────────────────────────────────────
    // دموی متنی: مدیریت اسرار — بندهای V13.3.1 و V13.3.2
    // ─────────────────────────────────────────────────────────────
    public IActionResult SecretsVault() =>
        View("../Shared/DemoResult", TextDemo("v13-vault", "V13.3.1,V13.3.2",
            "اسرار باید در راهکار مدیریت اسرار ایجاد/ذخیره/کنترل شوند و دسترسی کمینه باشد.", "V13",
            "نمونه خطر در همین پروژه: فایل appsettings.json یک کلید جعلی FakeApiKey دارد؛ اگر یک کلید «واقعی» آنجا باشد، " +
            "با هر کامیت گیت لو می‌رود (میلیون‌ها کلید نشتی در گیت‌هاب ردیف نشده‌اند!).\n\n" +
            "سلسله‌مراتب درست در .NET:\n" +
            "۱) توسعه: dotnet user-secrets set \"ApiKey\" \"...\"   (خارج از سورس)\n" +
            "۲) استیجینگ/تولید: متغیر محیطی یا Azure Key Vault / HashiCorp Vault\n" +
            "۳) دسترسی به Vault با هویت سرویس و «کمترین دسترسی» (بند V13.3.2)\n\n" +
            "الگوی خواندن:\n" +
            "var apiKey = builder.Configuration[\"ApiKey\"]; // از user-secrets/env/Vault\n\n" +
            "اجرای واقعی Vault به زیرساخت نیاز دارد؛ در این پروژه صرفاً کلید جعلی برای «نمایش توضیح» گذاشته شده است."));

    // ─────────────────────────────────────────────────────────────
    // دموی متنی: چرخش اسرار — بند V13.1.4
    // ─────────────────────────────────────────────────────────────
    public IActionResult SecretRotation() =>
        View("../Shared/DemoResult", TextDemo("v13-rotate", "V13.1.4",
            "اسرار امنیتی بحرانی و برنامه چرخش آنها باید مستند و اجرا شوند.", "V13",
            "برنامه چرخش نمونه:\n" +
            "• کلید API خارجی: هر ۹۰ روز + فوری پس از خروج هر کارمند با دسترسی\n" +
            "• کلید امضای JWT: چرخش با هم‌وجود کلید قدیم/جدید (اعتبارسنجی هر دو در دوره گذار)\n" +
            "• گواهی TLS: خودکار با ACME (Let's Encrypt)\n\n" +
            "چرخش بی‌دردسر مستلزم: اسرار فقط از پیکربندی خوانده شوند (نه هاردکد)، restart-graceful ممکن باشد و " +
            "نسخه‌بندی کلید (key id) در توکن‌ها/امضاها باشد (بند V11.1.1).\n\n" +
            "این الزام ماهیتاً «عملیاتی» است؛ در یک اپ آموزشی اجرای واقعی ندارد و طبق خواسته شما متنی ارائه شد."));
}
