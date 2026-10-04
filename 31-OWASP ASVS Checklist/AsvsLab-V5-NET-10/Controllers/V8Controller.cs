using AsvsLab.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AsvsLab.Controllers;

// ═══════════════════════════════════════════════════════════════════
//  فصل V8 — کنترل دسترسی / مجوزدهی (Authorization)
//  دموی IDOR با فاکتورهای واقعی کاربران وارده‌شده به سیستم اجرا می‌شود
// ═══════════════════════════════════════════════════════════════════
public class V8Controller : DemoBaseController
{
    private readonly DemoUserStore _store;
    public V8Controller(AsvsRefService asvs, ProgressService progress, DemoUserStore store)
        : base(asvs, progress) => _store = store;

    public IActionResult Index() =>
        SectionIndex(8, "کنترل دسترسی سطح تابع و سطح داده. دموی IDOR نیازمند ورود است — با ali/Ali12345! وارد شوید و سعی کنید فاکتور دیگری را ببینید.");

    // ─────────────────────────────────────────────────────────────
    // دموی ۱: IDOR — بند V8.2.2 (فرم تعاملی، نیازمند ورود)
    // ─────────────────────────────────────────────────────────────
    [HttpGet]
    public IActionResult Idor()
    {
        if (!User.Identity!.IsAuthenticated)
            return RedirectToAction("Login", "Account", new { returnUrl = "/V8/Idor" });
        return FormView("v8-idor", "id", "شناسه فاکتور برای مشاهده", "مثال: 1001 یا 1002 — فاکتورهای شما را در پروفایل ببینید");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Idor(int id, bool checkOn = true)
    {
        var r = New("v8-idor", "V8.2.2",
            "دسترسی سطح داده فقط برای مصرف‌کنندگان دارای مجوز صریح روی «همان داده» مجاز است (ضد IDOR).", "V8");
        r.VulnerableCode = """
            // ❌ ناامن: فقط «وجود داشتن» فاکتور بررسی می‌شود، نه مالکیت!
            var invoice = db.Invoices.Find(id);
            return View(invoice);
            // کاربر ali می‌تواند id=1001 (فاکتور مدیر) را ببیند!
            """;
        r.SecureCode = """
            // ✅ امن: فیلتر روی مالکیت داده در لایه سرویس
            var invoice = db.Invoices
                .Where(i => i.Id == id && i.OwnerUserId == currentUserId)
                .FirstOrDefault();
            if (invoice is null) return NotFound(); // حتی وجودش هم لو نمی‌رود
            """;

        var currentUser = _store.FindByName(User.Identity!.Name!);
        if (currentUser is null) return RedirectToAction("Login", "Account");

        var invoice = _store.FindInvoice(id);
        r.OutputLines.Add($"کاربر جاری: {currentUser.FullName} (شناسه داخلی {currentUser.Id})");
        r.OutputLines.Add($"فاکتورهای متعلق به شما: {string.Join("، ", _store.InvoicesOf(currentUser.Id).Select(i => i.Id))}");

        if (invoice is null)
        {
            r.OutputLines.Add($"🚫 فاکتور {id} وجود ندارد → پاسخ 404 (نه 403! تا وجود فاکتور لو نرود)");
        }
        else if (checkOn)
        {
            var owned = invoice.OwnerUserId == currentUser.Id;
            r.OutputLines.Add($"درخواست مشاهده فاکتور {id} با «بررسی مالکیت فعال»:");
            r.OutputLines.Add(owned
                ? $"✅ فاکتور شماست: {invoice.Description} — مبلغ {invoice.Amount:N0} تومان"
                : "🚫 این فاکتور متعلق به شما نیست → دسترسی رد شد (404)");
        }
        else
        {
            r.OutputLines.Add($"⚠️ حالت آسیب‌پذیر (بررسی مالکیت خاموش): فاکتور {invoice.OwnerName} — {invoice.Description} — مبلغ {invoice.Amount:N0} تومان به شما نمایش داده شد! این دقیقاً همان حمله IDOR است.");
            r.OutputLines.Add("یک بار دیگر با تیک «بررسی مالکیت» فعال امتحان کنید.");
        }
        ViewBag.CheckOn = checkOn;
        ViewBag.InvoiceId = id;
        return View("../Shared/DemoResult", r);
    }

    // ─────────────────────────────────────────────────────────────
    // دموی ۲: کنترل دسترسی سطح تابع — بند V8.2.1
    // ─────────────────────────────────────────────────────────────
    public IActionResult FunctionLevel()
    {
        var r = New("v8-function", "V8.2.1",
            "توابع مدیریتی فقط برای مصرف‌کنندگان دارای مجوز صریح در دسترس باشند.", "V8");
        r.VulnerableCode = """
            // ❌ ناامن: منوی ادمین فقط در UI پنهان شده!
            @if (isAdmin) { <a href="/Admin">پنل مدیریت</a> }
            // مهاجم مستقیم /Admin را در URL می‌زند
            """;
        r.SecureCode = """
            // ✅ امن: بررسی در سرور با Attribute (همین برنامه)
            [Authorize(Roles = "Admin")]
            public IActionResult AdminPanel() { ... }
            // + جلوگیری از شناسایی نقش با پیام عمومی 403
            """;
        var isAdmin = User.IsInRole("Admin");
        r.OutputLines.Add($"نقش شما: {User.Identity!.Name ?? "مهمان (وارد نشده)"}");
        r.OutputLines.Add(isAdmin
            ? "✅ نقش Admin دارید → اکشن AdminPanel برای شما باز است: /V8/AdminPanel"
            : "🚫 نقش Admin ندارید → درخواست به /V8/AdminPanel با 403 رد می‌شود. برای آزمایش با admin/Admin123! وارد شوید.");
        r.OutputLines.Add("خودِ اکشن AdminPanel این کنترلر با [Authorize(Roles=\"Admin\")] محافظت شده — مستقیم امتحان کنید!");
        return View("../Shared/DemoResult", r);
    }

    // اکشن واقعی محافظت‌شده برای آزمایش بند V8.2.1
    [Authorize(Roles = "Admin")]
    public IActionResult AdminPanel()
    {
        var r = New("v8-function", "V8.2.1",
            "توابع مدیریتی فقط برای دارندگان مجوز صریح.", "V8");
        r.OutputLines.Add("✅ به پنل مدیریت رسیدید — چون نقش Admin دارید.");
        r.OutputLines.Add($"تعداد کل کاربران ثبت‌شده: {_store.TotalUsers()}");
        return View("../Shared/DemoResult", r);
    }

    // ─────────────────────────────────────────────────────────────
    // دموی متنی: لایه مورد اعتماد — بند V8.3.1
    // ─────────────────────────────────────────────────────────────
    public IActionResult TrustedLayer() =>
        View("../Shared/DemoResult", TextDemo("v8-layer", "V8.3.1",
            "قواعد مجوز باید در لایه سرویس سمت سرور اعمال شوند، نه در کد قابل دستکاری کلاینت.", "V8",
            "سه لایه رایج اشتباه:\n\n" +
            "۱) جاوااسکریپت: if(!isAdmin) hideAdminMenu() → با DevTools دور زده می‌شود\n" +
            "۲) فرانت‌اند: مخفی کردن دکمه → درخواست مستقیم با curl باز هم می‌رسد\n" +
            "۳) سرور با اعتماد به ورودی: role=admin در body/کوکی → مهاجم مقدار می‌فرستد\n\n" +
            "الگوی درست (همین برنامه): Claims نقش از کوکی امضاشده سرور می‌آید و [Authorize] در سرور بررسی می‌کند؛ " +
            "در دموی IDOR هم فیلتر مالکیت داخل LINQ سمت سرور اعمال شد.\n\n" +
            "اصل تکمیلی: «Default Deny» — همه مسیرها پیش‌فرض ممنوع باشند مگر صراحتاً مجاز شده باشند؛ " +
            "در ASP.NET Core می‌توانید policy سراسری FallbackPolicy = RequireAuthenticatedUser بگذارید."));
}
