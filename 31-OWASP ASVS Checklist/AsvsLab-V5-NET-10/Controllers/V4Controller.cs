using AsvsLab.Models;
using AsvsLab.Services;
using Microsoft.AspNetCore.Mvc;

namespace AsvsLab.Controllers;

// ═══════════════════════════════════════════════════════════════════
//  فصل V4 — امنیت API و سرویس وب (API and Web Service)
// ═══════════════════════════════════════════════════════════════════
public class V4Controller : DemoBaseController
{
    public V4Controller(AsvsRefService asvs, ProgressService progress) : base(asvs, progress) { }

    public IActionResult Index() =>
        SectionIndex(4, "این فصل الزامات مربوط به Content-Type، متدهای HTTP، هدایت HTTPS و WebSocket را پوشش می‌دهد. دموی اعتبارسنجی مدل، رفتار واقعی یک endpoint API را شبیه‌سازی می‌کند.");

    // ─────────────────────────────────────────────────────────────
    // دموی ۱: هدر Content-Type — بند V4.1.1 (اعتبارسنجی مدل به‌سبک API)
    // ─────────────────────────────────────────────────────────────
    [HttpGet] public IActionResult ContentType() => FormView("v4-content", "q", "تعداد سفارش (برای endpoint شبیه‌سازی‌شده)", "عدد ۱ تا ۱۰۰ — مثلاً 500");

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ContentType(string q)
    {
        var r = New("v4-content", "V4.1.1",
            "هر پاسخ دارای بدنه باید Content-Type سازگار با محتوای واقعی داشته باشد؛ خطاهای اعتبارسنجی هم با ساختار مشخص برگردند.", "V4");
        r.VulnerableCode = """
            // ❌ ناامن: پاسخ خطا با HTML برای کلاینت JSON
            return Content("<h1>خطا!</h1> تعداد نامعتبر است", "text/html");
            """;
        r.SecureCode = """
            // ✅ امن: اعتبارسنجی مدل + پاسخ JSON با Content-Type درست و 400
            if (!ModelState.IsValid)
                return BadRequest(new { errors = ModelState.ToErrorList() }); // application/json
            """;

        var order = new ApiOrderViewModel { Product = "server" };
        var parsed = int.TryParse(q, out var qty);
        order.Quantity = parsed ? qty : 0;

        r.OutputLines.Add("شبیه‌سازی POST /api/order با Content-Type: application/json");
        r.OutputLines.Add($"بدنه: {{ \"product\": \"{order.Product}\", \"quantity\": {order.Quantity} }}");
        if (!parsed)
            r.OutputLines.Add("🚫 پاسخ 400 — «quantity» عددی نیست (پیام خطا با ساختار JSON و Content-Type درست)");
        else if (order.Quantity < 1 || order.Quantity > 100)
            r.OutputLines.Add($"🚫 پاسخ 400 — «{order.Quantity}» خارج از محدوده مجاز ۱..۱۰۰ (خطای [Range])");
        else
            r.OutputLines.Add($"✅ پاسخ 200 — سفارش «{order.Product}» با تعداد {order.Quantity} ثبت شد");

        r.OutputLines.Add($"Content-Type پاسخ برنامه: {Response.ContentType ?? "text/html; charset=utf-8"}");
        r.OutputLines.Add("نکته: در ASP.NET Core، [ApiController] اعتبارسنجی [Required]/[Range] را خودکار به 400 تبدیل می‌کند.");
        return View("../Shared/DemoResult", r);
    }

    // ─────────────────────────────────────────────────────────────
    // دموی متنی: هدایت HTTPS و هدرهای لایه میانی — بندهای V4.1.2 و V4.1.3
    // ─────────────────────────────────────────────────────────────
    public IActionResult HttpsRedirect() =>
        View("../Shared/DemoResult", TextDemo("v4-https", "V4.1.2,V4.1.3",
            "نقاط API نباید خودکار از HTTP به HTTPS هدایت شوند (پاسخ خطا بدهند) و هدرهای ست‌شده توسط لایه میانی باید اعتبارسنجی شوند.", "V4",
            "چرا API نباید redirect کند؟ چون کلاینت‌های API معمولاً در درخواست بعدی بدنه POST را دوباره نمی‌فرستند " +
            "(نشت داده یا خطای منطقی) و شنودگر شبکه کافی است برای دیدن درخواست اول.\n\n" +
            "پیکربندی درست:\n" +
            "  app.UseHttpsRedirection();      // فقط برای صفحات کاربری\n" +
            "  app.UseHsts();                   // V3.4.1\n" +
            "  // هدرهایی مثل X-Forwarded-For فقط از پروکسی مورد اعتماد:\n" +
            "  builder.Services.Configure<ForwardedHeadersOptions>(o =>\n" +
            "      o.KnownProxies.Add(IPAddress.Parse(\"10.0.0.5\")));  // V4.1.3\n\n" +
            "اجرای واقعی این الزام به وب‌سرور معکوس (Nginx/IIS) و گواهی TLS نیاز دارد؛ برای همین به‌صورت متنی + پیکربندی ارائه شد."));

    // ─────────────────────────────────────────────────────────────
    // دموی متنی: محدودسازی متدهای HTTP — بند V4.1.4
    // ─────────────────────────────────────────────────────────────
    public IActionResult Methods() =>
        View("../Shared/DemoResult", TextDemo("v4-methods", "V4.1.4",
            "فقط متدهای HTTP که صراحتاً پشتیبانی می‌شوند باید پاسخ بگیرند؛ بقیه 405 بدهند.", "V4",
            "در ASP.NET Core به‌طور پیش‌فرض فقط متدهای تعریف‌شده پاسخ می‌گیرند و بقیه 405 برمی‌گردانند. " +
            "برای سخت‌گیری بیشتر می‌توانید میان‌افزار سفارشی بنویسید:\n\n" +
            "app.Use(async (ctx, next) =>\n" +
            "{\n" +
            "    var allowed = new[] { \"GET\", \"POST\", \"HEAD\", \"OPTIONS\" };\n" +
            "    if (!allowed.Contains(ctx.Request.Method))\n" +
            "    { ctx.Response.StatusCode = 405; ctx.Response.Headers.Allow = string.Join(\", \", allowed); return; }\n" +
            "    await next();\n" +
            "});\n\n" +
            "مثال واقعی: یک endpoint فقط-خواندنی با متد TRACE یا DELETE نباید هیچ اطلاعات مفیدی بدهد."));

    // ─────────────────────────────────────────────────────────────
    // دموی متنی: امنیت WebSocket — بندهای V4.4.1 و V4.4.2
    // ─────────────────────────────────────────────────────────────
    public IActionResult WebSockets() =>
        View("../Shared/DemoResult", TextDemo("v4-ws", "V4.4.1,V4.4.2",
            "اتصال WebSocket باید با WSS (رمزنگاری‌شده) باشد و در دست‌دادن اولیه، هدر Origin بررسی شود.", "V4",
            "الگوی درست در ASP.NET Core:\n\n" +
            "app.UseWebSockets(new WebSocketOptions\n" +
            "{\n" +
            "    AllowedOrigins = { \"https://asvslab.example.com\" }  // V4.4.2\n" +
            "});\n\n" +
            "اجرای واقعی به وب‌سرور پشتیبانی‌کننده WebSocket و کلاینت نیاز دارد؛ در محیط آموزشی فعلی فعال نشده است. " +
            "نکات کلیدی: توکن احراز هویت WebSocket را در URL نفرستید (بند V4.4.3 — از هدر/پروتکل استفاده کنید) " +
            "و برای سشن‌های اختصاصی، توکن را از طریق مسیر احراز هویت قبلی بگیرید (بند V4.4.4)."));
}
