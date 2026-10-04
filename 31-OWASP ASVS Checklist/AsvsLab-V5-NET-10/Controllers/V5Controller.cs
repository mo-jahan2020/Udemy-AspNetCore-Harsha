using AsvsLab.Services;
using Microsoft.AspNetCore.Mvc;

namespace AsvsLab.Controllers;

// ═══════════════════════════════════════════════════════════════════
//  فصل V5 — مدیریت فایل‌ها (File Handling)
//  پوشه DemoFiles کنار برنامه، دیتای واقعی دموی Path Traversal است
// ═══════════════════════════════════════════════════════════════════
public class V5Controller : DemoBaseController
{
    private readonly IWebHostEnvironment _env;
    public V5Controller(AsvsRefService asvs, ProgressService progress, IWebHostEnvironment env)
        : base(asvs, progress) => _env = env;

    public IActionResult Index() =>
        SectionIndex(5, "این فصل آپلود امن فایل، مقابله با Path Traversal و اجرا نشدن فایل‌های آپلودی را پوشش می‌دهد. دموی مسیر با فایل‌های واقعی در پوشه DemoFiles اجرا می‌شود.");

    private string DemoDir => Path.Combine(_env.ContentRootPath, "DemoFiles");

    // ─────────────────────────────────────────────────────────────
    // دموی ۱: Path Traversal — بند V5.3.2 (فرم تعاملی با فایل واقعی)
    // ─────────────────────────────────────────────────────────────
    [HttpGet] public IActionResult PathTraversal() => FormView("v5-traversal", "q", "نام فایل برای دانلود", "مثال: public-note.txt یا ../Program.cs");

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult PathTraversal(string q)
    {
        var r = New("v5-traversal", "V5.3.2",
            "به‌جای نام فایل کاربر، از داده داخلی استفاده شود؛ در غیر این صورت مسیر باید کامل اعتبارسنجی شود.", "V5");
        r.VulnerableCode = """
            // ❌ ناامن: چسباندن مستقیم ورودی کاربر به مسیر
            var path = Path.Combine(appDir, "uploads", userInput);
            // ورودی: ../../Program.cs → خواندن هر فایل سرور!
            """;
        r.SecureCode = """
            // ✅ امن: پاک‌سازی نام + بررسی نهایی مسیر کاننیکال
            var safeName = Path.GetFileName(userInput);            // ../ را می‌برد
            var full = Path.GetFullPath(Path.Combine(uploads, safeName));
            if (!full.StartsWith(uploadsBase)) throw new UnauthorizedAccessException();
            """;

        var input = string.IsNullOrWhiteSpace(q) ? "public-note.txt" : q;
        var rawPath = Path.Combine(DemoDir, input);            // مسیر ناامن
        var vulnExists = System.IO.File.Exists(rawPath);
        var safeName = Path.GetFileName(input);                // پاک‌سازی
        var safePath = Path.GetFullPath(Path.Combine(DemoDir, safeName));
        var outside = !safePath.StartsWith(Path.GetFullPath(DemoDir));
        var safeContent = System.IO.File.Exists(safePath) && !outside
            ? System.IO.File.ReadAllText(safePath).Trim()
            : null;

        r.OutputLines.Add($"ورودی کاربر: «{input}»");
        r.OutputLines.Add($"❌ مسیر مستقیم: {rawPath} → {(vulnExists ? "فایل خوانده شد! محتوا: " + System.IO.File.ReadAllText(rawPath).Trim() : "فایل پیدا نشد (این‌بار محافظتِ اتفاقی سیستم فایل) — اما مسیر هرگز نباید به ورودی واگذار شود")}");
        r.OutputLines.Add($"✅ پس از Path.GetFileName: «{safeName}» → مسیر: {safePath}");
        r.OutputLines.Add(outside
            ? "🚫 مسیر نهایی خارج از پوشه مجاز است → درخواست رد شد"
            : safeContent is not null
                ? $"✅ فایل مجاز خوانده شد: «{safeContent}»"
                : "فایل مورد نظر در پوشه مجاز یافت نشد (فقط public-note.txt موجود است)");
        return View("../Shared/DemoResult", r);
    }

    // ─────────────────────────────────────────────────────────────
    // دموی ۲: آپلود امن — بندهای V5.1.1 و V5.2.1 (فرم آپلود واقعی)
    // ─────────────────────────────────────────────────────────────
    [HttpGet] public IActionResult SafeUpload() =>
        FormView("v5-upload", "file", "انتخاب فایل برای آپلود آزمایشی", "", isUpload: true,
            hint: "قواعد دمو: فقط پسوند png/jpg/pdf، حداکثر ۱۰۰ کیلوبایت. فایل هیچ‌جا ذخیره نمی‌شود؛ فقط اعتبارسنجی می‌شود.");

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SafeUpload(IFormFile? file)
    {
        var r = New("v5-upload", "V5.1.1,V5.2.1",
            "انواع فایل مجاز، پسوندها و اندازه حداکثر باید تعریف و در سمت سرور بررسی شوند.", "V5");
        r.VulnerableCode = """
            // ❌ ناامن: پذیرش هر فایلی با هر اندازه
            using var fs = System.IO.File.Create(Path.Combine(uploads, file.FileName));
            await file.CopyToAsync(fs);
            // webshell.aspx یا فایل ۲ گیگابایتی هم پذیرفته می‌شود!
            """;
        r.SecureCode = """
            // ✅ امن: پسوند مجاز + اندازه + بررسی امضای محتوا (Magic Bytes)
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext is not (".png" or ".jpg" or ".pdf")) return BadRequest("نوع فایل مجاز نیست");
            if (file.Length > 100 * 1024) return BadRequest("حجم فایل بیش از حد مجاز است");
            // و بررسی Magic Bytes: PNG با 0x89 0x50 0x4E 0x47 شروع می‌شود
            """;

        if (file is null || file.Length == 0)
        {
            r.Success = false;
            r.OutputLines.Add("هیچ فایلی ارسال نشد.");
            return View("../Shared/DemoResult", r);
        }

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var extOk = ext is ".png" or ".jpg" or ".jpeg" or ".pdf";
        var sizeOk = file.Length <= 100 * 1024;

        var head = new byte[8];
        using (var s = file.OpenReadStream()) await s.ReadExactlyAsync(head);
        var magic = ext switch
        {
            ".png" => head.Take(4).SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47 }),
            ".jpg" or ".jpeg" => head[0] == 0xFF && head[1] == 0xD8,
            ".pdf" => head.Take(4).SequenceEqual("%PDF"u8.ToArray()),
            _ => false
        };

        r.OutputLines.Add($"نام فایل: {file.FileName} | حجم: {file.Length:N0} بایت | نوع اعلامی: {file.ContentType}");
        r.OutputLines.Add($"۱) پسوند: {(extOk ? "✅ مجاز" : "🚫 غیرمجاز")} | ۲) اندازه ≤۱۰۰KB: {(sizeOk ? "✅" : "🚫")} | ۳) امضای محتوا (Magic Bytes): {(magic ? "✅ منطبق" : "🚫 ناسازگار")}");
        r.OutputLines.Add($"امضای خوانده‌شده: {Convert.ToHexString(head)}");
        r.OutputLines.Add(extOk && sizeOk && magic
            ? "✅ فایل همه بررسی‌ها را گذراند (در دموی واقعی همین‌جا با نام تولیدی سمت سرور ذخیره می‌شد — بند V5.3.2)"
            : "🚫 آپلود رد شد — دقیقاً همین بررسی‌ها جلوی webshell/فایل جعلی/حمله حجمی را می‌گیرند");
        return View("../Shared/DemoResult", r);
    }

    // ─────────────────────────────────────────────────────────────
    // دموی متنی: فایل آپلودی نباید اجرا شود — بند V5.3.1
    // ─────────────────────────────────────────────────────────────
    public IActionResult PublicFolder() =>
        View("../Shared/DemoResult", TextDemo("v5-public", "V5.3.1",
            "فایل‌های آپلودی در پوشه عمومی هرگز نباید به‌عنوان کد سمت سرور اجرا شوند.", "V5",
            "پیکربندی‌های کاربردی:\n\n" +
            "۱) Nginx — غیرفعال کردن اجرای PHP در پوشه آپلود:\n" +
            "   location ^~ /uploads/ { php_admin_flag engine off; }\n\n" +
            "۲) IIS/web.config — حذف handler ها:\n" +
            "   <handlers><clear /></handlers>\n" +
            "   <remove name=\"ASPNET_ISAPI\" />\n\n" +
            "۳) روش بهتر: فایل‌ها را خارج از webroot ذخیره کنید و با یک اکشن کنترلر (با بررسی مجوز) به‌صورت استریم سرو کنید؛ " +
            "نام فایل ذخیره‌شده را خودتان (GUID) بسازید و نام اصلی را فقط در Content-Disposition بگذارید.\n\n" +
            "اجرای واقعی به وب‌سرور و پیکربندی سرور نیاز دارد؛ در این برنامه فایلی در پوشه عمومی ذخیره نمی‌شود (دموی آپلود بالا فقط اعتبارسنجی می‌کند)."));
}
