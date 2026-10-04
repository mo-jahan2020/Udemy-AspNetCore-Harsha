using AsvsLab.Models;
using AsvsLab.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace AsvsLab.Controllers;

// ═══════════════════════════════════════════════════════════════════
//  فصل V1 — رمزگذاری و پاک‌سازی (Encoding and Sanitization)
//  در ASVS 5.0 دفاع در برابر تزریق‌ها (SQL/OS) به این فصل منتقل شده است
// ═══════════════════════════════════════════════════════════════════
public class V1Controller : DemoBaseController
{
    public V1Controller(AsvsRefService asvs, ProgressService progress) : base(asvs, progress) { }

    public IActionResult Index() =>
        SectionIndex(1, "در ASVS 5.0 این فصل شامل رمزگذاری ورودی/خروجی و دفاع در برابر تزریق‌هاست. دموی تزریق SQL با دیتابیس واقعی SQLite درون‌حافظه‌ای اجرا می‌شود.");

    // ─────────────────────────────────────────────────────────────
    // دموی ۱: تزریق SQL — بند V1.2.4 (کوئری پارامتری الزامی است)
    // ─────────────────────────────────────────────────────────────
    [HttpGet] public IActionResult SqlInjection() => FormView("v1-sqli", "q", "نام کاربری برای جستجو", "مثال: ali یا ' OR 1=1 --");

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SqlInjection(string q)
    {
        var r = New("v1-sqli", "V1.2.4",
            "تزریق SQL با تلفیق (Concatenate) رشته‌ها ممکن می‌شود؛ فقط کوئری پارامتری/ORM مجاز است.", "V1");
        r.VulnerableCode = """
            // ❌ ناامن: تلفیق مستقیم ورودی کاربر در کوئری
            var sql = "SELECT * FROM users WHERE username = '" + userInput + "'";
            // ورودی:  ' OR 1=1 --   ← کل جدول برمی‌گردد!
            """;
        r.SecureCode = """
            // ✅ امن: کوئری پارامتری — ورودی هرگز به کد SQL تفسیر نمی‌شود
            var sql = "SELECT * FROM users WHERE username = @u";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@u", userInput);
            """;

        using var conn = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=:memory:");
        conn.Open();
        var cmd0 = conn.CreateCommand();
        cmd0.CommandText = """
            CREATE TABLE users (id INTEGER, username TEXT, password TEXT, role TEXT);
            INSERT INTO users VALUES (1,'admin','s3cr3t-Hash','Admin'),
                                     (2,'ali','AliHash-x9','User');
            """;
        cmd0.ExecuteNonQuery();

        var input = string.IsNullOrWhiteSpace(q) ? "' OR 1=1 --" : q;

        // ۱) اجرای کوئری ناامن
        var vuln = conn.CreateCommand();
        vuln.CommandText = $"SELECT id, username, role FROM users WHERE username = '{input}'";
        var vulnRows = new List<string>();
        using (var reader = vuln.ExecuteReader())
            while (reader.Read())
                vulnRows.Add($"id={reader.GetInt32(0)} user={reader.GetString(1)} role={reader.GetString(2)}");

        // ۲) اجرای کوئری پارامتری امن
        var safe = conn.CreateCommand();
        safe.CommandText = "SELECT id, username, role FROM users WHERE username = @u";
        safe.Parameters.AddWithValue("@u", input);
        var safeRows = new List<string>();
        using (var reader = safe.ExecuteReader())
            while (reader.Read())
                safeRows.Add($"id={reader.GetInt32(0)} user={reader.GetString(1)} role={reader.GetString(2)}");

        r.OutputLines.Add($"ورودی مهاجم: {input}");
        r.OutputLines.Add($"❌ کوئری تلفیقی → {vulnRows.Count} ردیف برگشت: {(vulnRows.Count > 0 ? string.Join(" | ", vulnRows) : "—")}");
        r.OutputLines.Add($"✅ کوئری پارامتری → {safeRows.Count} ردیف برگشت: {(safeRows.Count > 0 ? string.Join(" | ", safeRows) : "—")}");
        r.OutputLines.Add("نتیجه: پارامتری کردن ورودی، payload مهاجم را به «متن ساده» تبدیل می‌کند.");
        return View("../Shared/DemoResult", r);
    }

    // ─────────────────────────────────────────────────────────────
    // دموی ۲: رمزگذاری خروجی — بندهای V1.1.2 و V1.2.1
    // ─────────────────────────────────────────────────────────────
    public IActionResult OutputEncoding()
    {
        var r = New("v1-encode", "V1.1.2,V1.2.1",
            "خروجی باید در آخرین مرحله و «وابسته به بافت» (HTML، اتریبیوت، URL، جاوااسکریپت) رمزگذاری شود.", "V1");
        r.VulnerableCode = """
            // ❌ ناامن: چاپ مستقیم ورودی کاربر در HTML
            @Html.Raw(userComment)
            // اگر ورودی <script>alert(1)</script> باشد → اجرای XSS
            """;
        r.SecureCode = """
            // ✅ امن: رمزگذاری وابسته به بافت (Razor به‌طور پیش‌فرض انکد می‌کند)
            @userComment
            // یا برای بافت اتریبیوت:
            @Html.AttributeEncode(userComment)
            """;

        var payload = "<script>alert('XSS')</script>";
        r.OutputLines.Add($"ورودی خام:        {payload}");
        r.OutputLines.Add($"HTML Encode شده:  {System.Net.WebUtility.HtmlEncode(payload)}");
        r.OutputLines.Add($"URL Encode شده:   {Uri.EscapeDataString(payload)}");
        r.OutputLines.Add("نتیجه: مرورگر کاراکترهای ویژه را «متن» می‌بیند و اسکریپت اجرا نمی‌شود.");
        return View("../Shared/DemoResult", r);
    }

    // ─────────────────────────────────────────────────────────────
    // دموی ۳: کاننیکالیزاسیون — بند V1.1.1
    // ─────────────────────────────────────────────────────────────
    public IActionResult Canonicalization()
    {
        var r = New("v1-canon", "V1.1.1",
            "ورودی انکدشده فقط یک‌بار و فقط در صورت انتظار داده انکدشده، به فرم کاننیکال رمزگشایی شود.", "V1");
        r.VulnerableCode = """
            // ❌ ناامن: رمزگشایی چندباره + تصمیم امنیتی روی فرم غیرکاننیکال
            var path = Server.MapPath(userInput);          // "%2e%2e%2fsecret"
            if (!path.Contains("../")) ReadFile(path);     // فیلتر دور زده می‌شود!
            """;
        r.SecureCode = """
            // ✅ امن: یک‌بار Decode → کاننیکال کردن → بعد بررسی مسیر
            var decoded = Uri.UnescapeDataString(userInput);
            var full = Path.GetFullPath(Path.Combine(baseDir, decoded));
            if (!full.StartsWith(baseDir)) throw new UnauthorizedAccessException();
            """;

        var once = "%2e%2e%2fsecret.txt";
        var twice = Uri.EscapeDataString(once);
        r.OutputLines.Add($"انکد دوگانه:      {twice}");
        r.OutputLines.Add($"یک‌بار Decode:    {Uri.UnescapeDataString(twice)}");
        r.OutputLines.Add($"دوبار Decode:     {Uri.UnescapeDataString(Uri.UnescapeDataString(twice))}");
        r.OutputLines.Add("نتیجه: اگر فیلتر فقط «../» خام را ببندد، مهاجم با انکد دوگانه دورش می‌زند.");
        return View("../Shared/DemoResult", r);
    }

    // ─────────────────────────────────────────────────────────────
    // دموی متنی: تزریق دستور سیستم‌عامل — بند V1.2.5
    // ─────────────────────────────────────────────────────────────
    public IActionResult CommandInjection() =>
        View("../Shared/DemoResult", TextDemo("v1-cmd", "V1.2.5",
            "فراخوانی سیستم‌عامل باید پارامتری یا با انکد وابسته به بافت انجام شود؛ تلفیق ورودی کاربر در دستور ممنوع است.", "V1",
            "در این برنامه هیچ فراخوانی واقعی سیستم‌عامل با ورودی کاربر انجام نمی‌شود (خودِ اجرای دستور مهاجم در محیط آموزشی خطرناک است). " +
            "الگوی درست: به‌جای اجرای «ping " + "userInput» از ProcessStartInfo با ArgumentList استفاده کنید که هر آرگومان را جدا و بدون تفسیر shell ارسال می‌کند:\n\n" +
            "var psi = new ProcessStartInfo(\"ping\", \"-c\") { UseShellExecute = false };\n" +
            "psi.ArgumentList.Add(hostInput);  // ✅ جدا از خود دستور\n\n" +
            "بهترین راه حل: اگر می‌شود، اصلاً از فراخوانی shell صرف‌نظر کنید و از کتابخانه‌های دات‌نت (مثل System.Net.NetworkInformation.Ping) استفاده کنید."));
}
