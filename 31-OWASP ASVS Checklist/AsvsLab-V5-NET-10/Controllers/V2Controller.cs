using AsvsLab.Services;
using Microsoft.AspNetCore.Mvc;

namespace AsvsLab.Controllers;

// ═══════════════════════════════════════════════════════════════════
//  فصل V2 — اعتبارسنجی و منطق تجاری (Validation and Business Logic)
// ═══════════════════════════════════════════════════════════════════
public class V2Controller : DemoBaseController
{
    public V2Controller(AsvsRefService asvs, ProgressService progress) : base(asvs, progress) { }

    public IActionResult Index() =>
        SectionIndex(2, "این فصل درباره اعتبارسنجی ورودی با فهرست مجاز و سلامت «منطق تجاری» است: ترتیب عملیات، سقف‌ها و جلوگیری از ارسال تکراری.");

    // ─────────────────────────────────────────────────────────────
    // دموی ۱: اعتبارسنجی مثبت با فهرست مجاز — بندهای V2.1.1 و V2.2.1
    // ─────────────────────────────────────────────────────────────
    [HttpGet] public IActionResult Allowlist() => FormView("v2-allowlist", "q", "کد تخفیف (الگوی مجاز: OFF-XXXX)", "مثال: OFF-2026 یا off-!!");

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Allowlist(string q)
    {
        var r = New("v2-allowlist", "V2.1.1,V2.2.1",
            "اعتبارسنجی باید «مثبت» باشد: فقط الگوها/مقادیر مجاز پذیرفته شوند؛ اعتبارسنجی در لایه سرویس سمت سرور انجام می‌شود.", "V2");
        r.VulnerableCode = """
            // ❌ ناامن: اعتبارسنجی «منفی» — فقط چیزهای بد را رد می‌کند
            if (code.Contains("<") || code.Contains("'")) Reject();
            Accept(code); // هزاران payload دیگر از قلم می‌افتد
            """;
        r.SecureCode = """
            // ✅ امن: اعتبارسنجی «مثبت» با فهرست مجاز (Allow-list)
            private static readonly Regex Valid = new(@"^OFF-[A-Z0-9]{4}$",
                RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));
            if (!Valid.IsMatch(code)) return BadRequest("قالب کد تخفیف نامعتبر است");
            """;

        var input = string.IsNullOrWhiteSpace(q) ? "OFF-2026" : q;
        var ok = System.Text.RegularExpressions.Regex.IsMatch(input, @"^OFF-[A-Z0-9]{4}$");
        r.OutputLines.Add($"ورودی: «{input}»");
        r.OutputLines.Add(ok
            ? "✅ با الگوی مجاز (^OFF-[A-Z0-9]{4}$) مطابقت دارد → پردازش ادامه می‌یابد"
            : "🚫 با الگوی مجاز مطابقت ندارد → درخواست رد می‌شود (BadRequest 400)");
        r.OutputLines.Add("نکته: در فهرست مجاز، ورودی‌های «ناشناخته» به‌طور پیش‌فرض رد می‌شوند (Default Deny).");
        return View("../Shared/DemoResult", r);
    }

    // ─────────────────────────────────────────────────────────────
    // دموی ۲: منطق تجاری — سقف‌ها و مقادیر نامعتبر — بند V2.3.2
    // ─────────────────────────────────────────────────────────────
    public IActionResult NegativeAmount()
    {
        var r = New("v2-negative", "V2.3.2",
            "سقف‌های منطق تجاری (مثل مبلغ تراکنش) باید مطابق مستندات اعمال شود تا از سوءاستفاده جلوگیری شود.", "V2");
        r.VulnerableCode = """
            // ❌ ناامن: بدون بررسی سقف — مهاجم مبلغ منفی می‌فرستد
            account.Balance -= request.Amount;
            // Amount = -1,000,000 → موجودی «افزایش» می‌یابد!
            """;
        r.SecureCode = """
            // ✅ امن: اعتبارسنجی دامنه در لایه سرویس
            if (request.Amount <= 0 || request.Amount > account.Balance)
                return UnprocessableEntity("مبلغ تراکنش نامعتبر است");
            account.Balance -= request.Amount;
            """;

        decimal balance = 5_000_000m;
        var attempts = new[] { 1_000_000m, -500_000m, 9_999_999m };
        r.OutputLines.Add($"موجودی اولیه: {balance:N0} تومان");
        foreach (var amt in attempts)
        {
            var valid = amt > 0 && amt <= balance;
            r.OutputLines.Add(valid
                ? $"✅ برداشت {amt:N0} → تأیید شد (موجودی جدید: {balance - amt:N0})"
                : $"🚫 برداشت {amt:N0} → رد شد ({(amt <= 0 ? "مبلغ منفی یا صفر" : "بیش از موجودی")})");
        }
        r.OutputLines.Add("نکته: این سقف‌ها باید در «مستندات» هم تعریف شده باشند (بند V2.1.3).");
        return View("../Shared/DemoResult", r);
    }

    // ─────────────────────────────────────────────────────────────
    // دموی ۳: ارسال تکراری / ترتیب عملیات — بند V2.3.1
    // ─────────────────────────────────────────────────────────────
    public IActionResult DuplicateSubmit()
    {
        var r = New("v2-duplicate", "V2.3.1",
            "هر جریان تجاری فقط در توالی و تعداد مورد انتظار پردازش شود (ضد ارسال تکراری).", "V2");
        r.VulnerableCode = """
            // ❌ ناامن: هر POST یک تراکنش جدید ثبت می‌کند
            // (دابل‌کلیک کاربر یا Retry شبکه → کسر دوباره پول!)
            db.Orders.Add(order); db.SaveChanges();
            """;
        r.SecureCode = """
            // ✅ امن: کلید یکتایی (Idempotency Key) در سمت سرور
            if (_processedKeys.Contains(request.IdempotencyKey))
                return Ok("این تراکنش قبلاً پردازش شده است"); // خروجی همان نتیجه اول
            _processedKeys.Add(request.IdempotencyKey);
            db.Orders.Add(order); db.SaveChanges();
            """;

        var processed = new HashSet<string>();
        foreach (var key in new[] { "tx-1", "tx-1", "tx-1", "tx-2" })
        {
            if (processed.Contains(key))
                r.OutputLines.Add($"🔁 کلید «{key}» تکراری → دوباره پردازش نشد (پاسخ نتیجه اول)");
            else
            {
                processed.Add(key);
                r.OutputLines.Add($"🆕 کلید «{key}» → تراکنش ثبت شد");
            }
        }
        r.OutputLines.Add("نتیجه: با ۳ بار ارسال «tx-1» فقط یک بار پول کسر می‌شود.");
        return View("../Shared/DemoResult", r);
    }

    // ─────────────────────────────────────────────────────────────
    // دموی متنی: ضد اتوماسیون — بند V2.4.1
    // ─────────────────────────────────────────────────────────────
    public IActionResult AntiAutomation() =>
        View("../Shared/DemoResult", TextDemo("v2-antiauto", "V2.4.1",
            "در برابر فراخوانی بیش از حد توابع (سرقت داده، تولید داده زباله، تخلیه سهمیه) کنترل ضد اتوماسیون لازم است.", "V2",
            "راهکار عملی در .NET 10 — میان‌افزار Rate Limiting داخلی:\n\n" +
            "builder.Services.AddRateLimiter(o => o.AddFixedWindowLimiter(\"api\",\n" +
            "    opt => { opt.Window = TimeSpan.FromMinutes(1); opt.PermitLimit = 30; }));\n" +
            "app.UseRateLimiter();\n\n" +
            "در دموی واقعی، فعال‌سازی سراسری Rate Limiter با پاسخ‌های ۴۲۹ می‌تواند جریان عادی آموزش را قطع کند؛ " +
            "برای همین فقط الگوی کد ارائه شد. همچنین CAPTCHA برای فرم‌های عمومی و «زمان‌بندی انسانی» (بند V2.4.2) را در نظر بگیرید."));
}
