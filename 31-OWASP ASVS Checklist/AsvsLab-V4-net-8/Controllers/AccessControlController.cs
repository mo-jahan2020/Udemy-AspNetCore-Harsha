using AsvsLab.Models;
using AsvsLab.Services;
using Microsoft.AspNetCore.Mvc;

namespace AsvsLab.Controllers;

// ============================================================================
// 🚪 بخش سوم: کنترل دسترسی / مجوزدهی (Authorization)
// فصل V4 استاندارد OWASP ASVS 4.0.3
// ============================================================================
public class AccessControlController : Controller
{
    // ------------------------------------------------------------------------
    // فهرست دموهای این بخش (صفحه اول کنترلر)
    // ------------------------------------------------------------------------
    private static readonly SectionCatalog Catalog = new()
    {
        Title = "بخش ۳: کنترل دسترسی (Authorization)",
        ChapterRef = "فصل V4 — OWASP ASVS 4.0.3",
        Demos =
        {
            new() { Action = "DenyByDefault", Title = "انکار به‌صورت پیش‌فرض و حداقل دسترسی", AsvsRef = "V4.1.1 — V4.1.5" },
            new() { Action = "Idor", Title = "تست حمله IDOR روی فاکتورها (فرم ورودی)", AsvsRef = "V4.2.1" },
        }
    };

    [HttpGet]
    public IActionResult Index() => View("../Shared/SectionIndex", Catalog);

    // مدل کاربر آزمایشی برای دموها
    private record DemoUser(int Id, string Name, string Role);

    // کاربران آزمایشی: مهمان، مشتری عادی و مدیر
    private static readonly DemoUser Guest = new(0, "مهمان (لاگین نکرده)", "Guest");
    private static readonly DemoUser Ali = new(1, "علی رضایی", "Customer");
    private static readonly DemoUser Admin = new(2, "مدیر سیستم", "Admin");

    // ------------------------------------------------------------------------
    // 🎯 دمو ۱: رویکرد «انکار به‌صورت پیش‌فرض» (Deny by Default) و حداقل دسترسی
    //
    // ⚙️ بندهای استاندارد OWASP ASVS 4.0.3:
    //   V4.1.1 → قواعد کنترل دسترسی باید در لایه سرویسِ قابل اعتماد اعمال شود
    //            (نه فقط در UI؛ چون دکمه مخفی کردنِ UI را مهاجم دور می‌زند)
    //   V4.1.3 → اصل حداقل دسترسی (Least Privilege): کاربر فقط به منابعی
    //            دسترسی داشته باشد که مجوز واقعی برای آن‌ها دارد
    //   V4.1.5 → کنترل دسترسی باید هنگام بروز استثنا هم «امن» شکست بخورد
    // ------------------------------------------------------------------------
    [HttpGet]
    public IActionResult DenyByDefault()
    {
        var result = new DemoResult
        {
            Title = "انکار به‌صورت پیش‌فرض و اصل حداقل دسترسی",
            AsvsRef = "V4.1.1 — V4.1.3 — V4.1.5",
            AsvsSummary = "کنترل دسترسی در لایه سرویس اعمال شود، اصل حداقل دسترسی رعایت شود و کنترل‌ها هنگام خطا هم به‌صورت امن شکست بخورند.",
            Explanation = "بسیاری از برنامه‌ها فقط دکمه پنل مدیریت را از منوی کاربر عادی مخفی می‌کنند؛ اما مهاجم مستقیم آدرس را در مرورگر تایپ می‌کند! رویکرد صحیح این است که «به‌صورت پیش‌فرض همه چیز ممنوع باشد» و فقط دسترسی‌های صریحاً اعلام‌شده باز شوند، و بررسی در سمت سرور انجام شود. در این دمو سه کاربر (مهمان، مشتری، مدیر) تلاش می‌کنند وارد پنل مدیریت شوند؛ تابع بررسی فقط «Admin» را می‌شناسد و هر چیز ناشناخته‌ای (حتی نقش‌های تایپی‌خورده) را رد می‌کند.",
            CodeSnippet = """
                // انکار به‌صورت پیش‌فرض + شکست امن:
                private bool CanAccessAdminPanel(DemoUser? user)
                {
                    if (user is null) return false;              // لاگین نکرده → ممنوع
                    if (user.Role != "Admin") return false;      // غیر از Admin → ممنوع
                    return true;                                 // فقط صریحاً مجاز
                }

                // در برنامه واقعی با اتریبیوت در لایه کنترلر/سرویس:
                // [Authorize(Roles = "Admin")]
                """,
            BackController = "AccessControl"
        };

        // ------------------------- اجرای واقعی دمو -------------------------
        result.OutputLines.Add("تلاش سه کاربر برای ورود به «پنل مدیریت» (آدرس مستقیم):");
        result.OutputLines.Add("");

        foreach (var user in new[] { Guest, Ali, Admin })
        {
            // تابع بررسی: به‌صورت پیش‌فرض همه ممنوع‌اند؛ فقط Admin صریحاً مجاز است
            bool allowed = CanAccessAdminPanel(user);
            result.OutputLines.Add($"  کاربر: {user.Name} (نقش: {user.Role})");
            result.OutputLines.Add($"   → {(allowed ? "✓ مجاز — پنل مدیریت باز شد" : "⛔ ممنوع — دسترسی رد شد (HTTP 403) و در لاگ امنیتی ثبت شد")}");
            result.OutputLines.Add("");
        }

        // نمایش سناریوی «شکست امن» (V4.1.5): نقش ناشناخته یا داده خراب
        var corruptedUser = new DemoUser(3, "کاربر با داده خراب", "Adm1n ");
        result.OutputLines.Add("سناریوی شکست امن (V4.1.5): کاربری با نقش خراب «Adm1n» (فاصله + عدد) وارد می‌شود:");
        result.OutputLines.Add($"   → {(CanAccessAdminPanel(corruptedUser) ? "❌ متأسفانه مجاز شد!" : "✓ رد شد — چون بررسی دقیقاً «Admin» را می‌خواهد و هر چیز دیگری حتی با شباهت، ممنوع است")}");
        result.OutputLines.Add("");
        result.OutputLines.Add("✓ نتیجه: هیچ‌کس به‌طور پیش‌فرض مجاز نیست و نقش‌های ناشناخته/خراب هم به‌صورت امن رد می‌شوند.");

        return View("../Shared/DemoResult", result);
    }

    private static bool CanAccessAdminPanel(DemoUser? user) =>
        user is not null && user.Role == "Admin";

    // ------------------------------------------------------------------------
    // 🎯 دمو ۲ (فرم ورودی): مقابله با IDOR (اشاره‌مستقیم ناامن به اشیاء)
    //
    // ⚙️ بند استاندارد OWASP ASVS 4.0.3:
    //   V4.2.1 → داده‌ها و APIهای حساس باید در برابر حمله IDOR برای ساخت،
    //            خواندن، ویرایش و حذف رکوردها محافظت شوند
    // ------------------------------------------------------------------------
    [HttpGet]
    public IActionResult Idor(string? input)
    {
        const string asvsRef = "V4.2.1";
        const string summary = "برنامه باید در برابر IDOR محافظت کند؛ یعنی هنگام نمایش هر رکورد، مالکیت کاربر جاری را بررسی کند.";

        // کاربر جاریِ دمو: علی (شناسه ۱) — در برنامه واقعی از کوکی نشست می‌آید
        const int currentUserId = 1;

        if (string.IsNullOrEmpty(input))
        {
            return View("../Shared/DemoForm", new DemoFormViewModel
            {
                Title = "تست حمله IDOR روی فاکتورها",
                AsvsRef = asvsRef,
                AsvsSummary = summary,
                Description = "شما کاربر «علی رضایی» (شناسه ۱) هستید. فاکتور قانونی شما شماره 1001 است. حالا شماره 1002 یا 1003 (فاکتور دیگران) را وارد کنید و ببینید بررسی مالکیت چطور حمله را خنثی می‌کند. در برنامه آسیب‌پذیر، همین کار داده‌های مالی دیگران را لو می‌دهد!",
                InputLabel = "شماره فاکتور (مثلاً 1001 یا 1002)",
                FormAction = "/AccessControl/Idor",
                PresetValue = "1001"
            });
        }

        var result = new DemoResult
        {
            Title = "تست حمله IDOR روی فاکتورها",
            AsvsRef = asvsRef,
            AsvsSummary = summary,
            Explanation = "IDOR وقتی رخ می‌دهد که برنامه فقط «شناسه رکورد» را ببینند بپذیرد و بپرسد «رکورد وجود دارد؟» به‌جای «رکوردِ متعلق به این کاربر است؟». مهاجم با تغییر شناسه در URL (1001 → 1002) به داده بقیه دست می‌یابد. راه‌حل استاندارد: همیشه شرط مالکیت در خود کوئری/سرویس اعمال شود (WHERE Id = @id AND OwnerUserId = @userId).",
            CodeSnippet = """
                // ❌ آسیب‌پذیر: فقط شناسه چک می‌شود
                var invoice = db.Invoices.Find(id);

                // ✓ امن: مالکیت در همان کوئری اعمال می‌شود (بند V4.2.1)
                var invoice = db.Invoices
                    .FirstOrDefault(i => i.Id == id
                                      && i.OwnerUserId == currentUserId);
                if (invoice is null) return Forbid(); // 403 + ثبت در لاگ
                """,
            BackController = "AccessControl"
        };

        if (!int.TryParse(input, out int invoiceId))
        {
            result.Success = false;
            result.OutputLines.Add($"ورودی «{input}» عدد نیست؛ لطفاً یک شماره فاکتور عددی وارد کنید.");
            return View("../Shared/DemoResult", result);
        }

        var target = InvoiceRepository.Find(invoiceId);

        result.OutputLines.Add($"کاربر جاری: علی رضایی (شناسه {currentUserId})");
        result.OutputLines.Add($"درخواست مشاهده فاکتور شماره: {invoiceId}");
        result.OutputLines.Add(new string('-', 60));
        result.OutputLines.Add("【۱】 نسخه آسیب‌پذیر (فقط وجود رکورد را چک می‌کند):");

        if (target is not null)
        {
            // ❌ شبیه‌سازی رفتار آسیب‌پذیر: نمایش بدون بررسی مالکیت
            result.OutputLines.Add($"   ⚠️ فاکتور باز شد! مالک واقعی: {target.OwnerName}");
            result.OutputLines.Add($"   ⚠️ شرح: {target.Description} | مبلغ: {target.Amount:N0} ریال");
            if (target.OwnerUserId != currentUserId)
                result.OutputLines.Add("   🔴 نشت داده! فاکتورِ کاربر دیگری نمایش داده شد (حمله IDOR موفق بود)");
        }
        else
        {
            result.OutputLines.Add("   فاکتور پیدا نشد (404)");
        }

        result.OutputLines.Add("");
        result.OutputLines.Add("【۲】 نسخه امن (بررسی مالکیت مطابق بند V4.2.1):");

        if (target is not null && target.OwnerUserId == currentUserId)
        {
            result.OutputLines.Add($"   ✓ فاکتور شما: {target.Description} | مبلغ: {target.Amount:N0} ریال");
        }
        else
        {
            result.OutputLines.Add("   ⛔ دسترسی رد شد (HTTP 403 Forbidden)");
            result.OutputLines.Add("   ✓ این تصمیم در لاگ امنیتی ثبت شد: «کاربر X تلاش برای مشاهده فاکتور دیگران» (مرتبط با بند V7.2.2)");
        }

        return View("../Shared/DemoResult", result);
    }
}
