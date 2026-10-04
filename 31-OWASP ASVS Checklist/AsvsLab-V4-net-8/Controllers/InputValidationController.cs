using System.Text;
using System.Text.Encodings.Web;
using AsvsLab.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace AsvsLab.Controllers;

// ============================================================================
// 🧹 بخش چهارم: اعتبارسنجی، پاک‌سازی و انکدینگ ورودی
// فصل V5 استاندارد OWASP ASVS 4.0.3
// ============================================================================
public class InputValidationController : Controller
{
    // ------------------------------------------------------------------------
    // فهرست دموهای این بخش (صفحه اول کنترلر)
    // ------------------------------------------------------------------------
    private static readonly SectionCatalog Catalog = new()
    {
        Title = "بخش ۴: اعتبارسنجی و انکدینگ ورودی",
        ChapterRef = "فصل V5 — OWASP ASVS 4.0.3",
        Demos =
        {
            new() { Action = "SqlInjection", Title = "حمله SQL Injection — پارامتری در برابر آسیب‌پذیر (فرم ورودی)", AsvsRef = "V5.3.4 — V5.3.5" },
            new() { Action = "Xss", Title = "حمله XSS و انکدینگ خروجی (فرم ورودی)", AsvsRef = "V5.3.1 — V5.3.3" },
            new() { Action = "PathTraversal", Title = "حمله Path Traversal / خواندن فایل غیرمجاز (فرم ورودی)", AsvsRef = "V5.3.9" },
            new() { Action = "AllowList", Title = "اعتبارسنجی Allow-List روی کد ملی (فرم ورودی)", AsvsRef = "V5.1.3 — V5.1.4" },
        }
    };

    [HttpGet]
    public IActionResult Index() => View("../Shared/SectionIndex", Catalog);

    // ------------------------------------------------------------------------
    // 🎯 دمو ۱ (فرم ورودی): حملات SQL Injection — نسخه آسیب‌پذیر در برابر امن
    //
    // ⚙️ بندهای استاندارد OWASP ASVS 4.0.3:
    //   V5.3.4 → کوئری‌های دیتابیس باید Parameterized باشند یا از ORM/EF استفاده کنند
    //   V5.3.5 → اگر Parameterize ممکن نبود، از انکدینگ مخصوص متن استفاده شود
    //
    // ℹ️ این دمو از دیتابیس SQLite درون‌حافظه‌ای استفاده می‌کند؛
    //    نیازی به نصب هیچ سرویس دیتابیسی روی رایانه شما نیست.
    // ------------------------------------------------------------------------
    [HttpGet]
    public async Task<IActionResult> SqlInjection(string? input)
    {
        const string asvsRef = "V5.3.4 — V5.3.5";
        const string summary = "کوئری‌های دیتابیس باید پارامتری (Parameterized) باشند تا ورودی کاربر هرگز به‌عنوان کد SQL تفسیر نشود.";

        if (string.IsNullOrEmpty(input))
        {
            return View("../Shared/DemoForm", new DemoFormViewModel
            {
                Title = "حمله SQL Injection — آسیب‌پذیر در برابر امن",
                AsvsRef = asvsRef,
                AsvsSummary = summary,
                Description = "در جدول کاربران، نام کاربری را جستجو می‌کنیم. برای اثبات حمله، این مقدار را وارد کنید: ' OR '1'='1 و ببینید نسخه آسیب‌پذیر چطور کل جدول را لو می‌دهد، در حالی که نسخه پارامتری همان ورودی را فقط «متن معمولی» می‌داند.",
                InputLabel = "نام کاربری برای جستجو",
                FormAction = "/InputValidation/SqlInjection",
                PresetValue = "' OR '1'='1"
            });
        }

        var result = new DemoResult
        {
            Title = "حمله SQL Injection — آسیب‌پذیر در برابر امن",
            AsvsRef = asvsRef,
            AsvsSummary = summary,
            Explanation = "در کوئری آسیب‌پذیر، ورودی کاربر مستقیماً به متن دستور SQL چسبانده می‌شود؛ بنابراین مهاجم «دستور» می‌نویسد نه «مقدار». عبارت ' OR '1'='1 شرط WHERE را همیشه درست می‌کند و کل جدول برمی‌گردد — و این فقط ساده‌ترین حمله است؛ با '; DROP TABLE یا UNION SELECT می‌توان داده‌های جدول‌های دیگر را هم استخراج یا حذف کرد. در نسخه امن، مقدار ورودی به‌عنوان «پارامتر» جدا از دستور SQL ارسال می‌شود؛ درایور دیتابیس آن را فقط متن در نظر می‌گیرد و حمله بی‌اثر است.",
            CodeSnippet = """
                // ❌ آسیب‌پذیر: چسباندن ورودی به متن دستور SQL
                var sql = "SELECT * FROM Users WHERE Username = '" + userInput + "'";
                command.CommandText = sql;

                // ✓ امن: کوئری پارامتری (بند V5.3.4)
                command.CommandText = "SELECT * FROM Users WHERE Username = @u";
                command.Parameters.AddWithValue("@u", userInput);
                """,
            BackController = "InputValidation"
        };

        result.OutputLines.Add($"ورودی کاربر: {input}");
        result.OutputLines.Add(new string('=', 60));

        // اتصال SQLite درون‌حافظه‌ای (بدون نیاز به نصب سرویس)
        await using var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();

        // ساخت جدول نمونه و درج دو کاربر
        CreateDemoUsersTable(connection);

        // ------------------- بخش ۱: کوئری آسیب‌پذیر -------------------
        result.OutputLines.Add("【۱】 کوئری آسیب‌پذیر (چسباندن ورودی به متن SQL):");
        string vulnerableSql = $"SELECT Username, Email FROM Users WHERE Username = '{input}'";
        result.OutputLines.Add($"   دستور SQL ساخته‌شده: {vulnerableSql}");

        var vulnerableCommand = connection.CreateCommand();
        vulnerableCommand.CommandText = vulnerableSql;
        var foundRows = 0;
        await using (var reader = await vulnerableCommand.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                foundRows++;
                result.OutputLines.Add($"   🔴 ردیف لو رفته → نام: {reader.GetString(0)} | ایمیل: {reader.GetString(1)}");
            }
        }
        result.OutputLines.Add(foundRows > 1
            ? $"   🔴 حمله موفق بود! {foundRows} ردیف برگشت شد در حالی که باید جستجوی نام کاربری باشد!"
            : $"   نتیجه: {foundRows} ردیف برگشت شد.");

        result.OutputLines.Add("");

        // ------------------- بخش ۲: کوئری امن پارامتری -------------------
        result.OutputLines.Add("【۲】 کوئری امن پارامتری (بند V5.3.4):");
        var safeCommand = connection.CreateCommand();
        safeCommand.CommandText = "SELECT Username, Email FROM Users WHERE Username = @username";
        safeCommand.Parameters.AddWithValue("@username", input);

        var safeRows = 0;
        await using (var reader2 = await safeCommand.ExecuteReaderAsync())
        {
            while (await reader2.ReadAsync())
            {
                safeRows++;
                result.OutputLines.Add($"   ردیف → نام: {reader2.GetString(0)} | ایمیل: {reader2.GetString(1)}");
            }
        }
        result.OutputLines.Add($"   نتیجه: {safeRows} ردیف — ورودی فقط به‌عنوان «متن» جستجو شد، نه دستور SQL ✓");
        result.OutputLines.Add("");
        result.OutputLines.Add("✓ نتیجه‌گیری: همان ورودی، در نسخه آسیب‌پذیر کل جدول را لو داد و در نسخه پارامتری بی‌اثر بود.");

        return View("../Shared/DemoResult", result);
    }

    // ساخت دیتابیس نمونه درون‌حافظه‌ای
    private static void CreateDemoUsersTable(SqliteConnection connection)
    {
        var cmd = connection.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE Users (Username TEXT, Email TEXT);
            INSERT INTO Users VALUES ('ali', 'ali@example.com');
            INSERT INTO Users VALUES ('sara', 'sara@example.com');
            INSERT INTO Users VALUES ('admin', 'admin@example.com');
            """;
        cmd.ExecuteNonQuery();
    }

    // ------------------------------------------------------------------------
    // 🎯 دمو ۲ (فرم ورودی): حمله Cross-Site Scripting (XSS) و انکدینگ خروجی
    //
    // ⚙️ بندهای استاندارد OWASP ASVS 4.0.3:
    //   V5.3.1 → انکدینگ خروجی باید متناسب با زمینه (HTML, JS, URL و...) باشد
    //   V5.3.3 → خروجی امن، محافظت در برابر XSS بازتابی، ذخیره‌شده و DOM باشد
    // ------------------------------------------------------------------------
    [HttpGet]
    public IActionResult Xss(string? input)
    {
        const string asvsRef = "V5.3.1 — V5.3.3";
        const string summary = "خروجی باید در زمینه درست (HTML، جاوااسکریپت، URL و...) انکد شود تا ورودی مخرب هرگز به‌عنوان اسکریپت اجرا نشود.";

        if (string.IsNullOrEmpty(input))
        {
            return View("../Shared/DemoForm", new DemoFormViewModel
            {
                Title = "حمله XSS و انکدینگ خروجی",
                AsvsRef = asvsRef,
                AsvsSummary = summary,
                Description = "یک نظر (کامنت) وارد کنید. پیشنهاد حمله: <script>alert('XSS')</script> — سپس ببینید نسخه امن چطور آن را بی‌اثر می‌کند. توجه: Razor به‌صورت پیش‌فرض خودش انکد می‌کند؛ خطر جایی است که با Html.Raw از انکد صرف‌نظر کنید.",
                InputLabel = "متن نظر",
                FormAction = "/InputValidation/Xss",
                PresetValue = "<script>alert('XSS')</script>"
            });
        }

        var result = new DemoResult
        {
            Title = "حمله XSS و انکدینگ خروجی",
            AsvsRef = asvsRef,
            AsvsSummary = summary,
            Explanation = "XSS یعنی مهاجم «کد جاوااسکریپت» را به‌جای «متن» وارد برنامه کند تا در مرورگر قربانی اجرا شود؛ از این طریق می‌تواند کوکی نشست (اگر HttpOnly نباشد) را بدزدد، صفحه جعلی نشان دهد یا به‌جای کاربر درخواست بفرستد. راه‌حل: موقع نمایش، همه کاراکترهای ویژه (< > & ' ...) به معادل HTML انکد شوند تا «کد» به «متن ساده» تبدیل شود. در این دمو، خروجی امن با HtmlEncoder.Encode واقعی تولید شده است.",
            CodeSnippet = """
                // ✓ انکدینگ خروجی متناسب با زمینه HTML (بند V5.3.1)
                string safe = HtmlEncoder.Default.Encode(userComment);
                // <script> می‌شود: &lt;script&gt; → مرورگر آن را متن نشان می‌دهد، اجرا نمی‌کند

                // در Razor انکدینگ پیش‌فرض است:
                <div>@Model.UserComment</div>   ✓ امن
                <div>@Html.Raw(Model.UserComment)</div>  ❌ هرگز با ورودی کاربر!
                """,
            BackController = "InputValidation"
        };

        // ------------------------- اجرای واقعی دمو -------------------------
        string encoded = HtmlEncoder.Default.Encode(input);

        result.OutputLines.Add($"ورودی خام کاربر: {input}");
        result.OutputLines.Add("");
        result.OutputLines.Add("【۱】 نسخه آسیب‌پذیر — نمایش با Html.Raw (انکد نشده):");
        result.OutputLines.Add($"   ❌ اگر این متن با Html.Raw در صفحه قرار گیرد، مرورگر تگ <script> را «اجرا می‌کند»");
        result.OutputLines.Add($"   ❌ نتیجه: پاپ‌آپ alert اجرا می‌شود و مهاجم می‌تواند document.cookie را هم بخواند");
        result.OutputLines.Add("");
        result.OutputLines.Add("【۲】 نسخه امن — انکدینگ خروجی با HtmlEncoder (بند V5.3.1):");
        result.OutputLines.Add($"   متن پس از انکدینگ: {encoded}");
        result.OutputLines.Add("   ✓ مرورگر این متن را فقط «به‌صورت نوشتاری» نشان می‌دهد؛ هیچ کدی اجرا نمی‌شود");
        result.OutputLines.Add("");
        result.OutputLines.Add($"تست واقعی: آیا انکدینگ، تگ بازکننده را خنثی کرد؟ {(encoded.Contains("&lt;") ? "بله ✓ (کاراکتر < به &lt; تبدیل شد)" : "خیر")}");

        return View("../Shared/DemoResult", result);
    }

    // ------------------------------------------------------------------------
    // 🎯 دمو ۳ (فرم ورودی): حمله Path Traversal (پیمایش مسیر)
    //
    // ⚙️ بند استاندارد OWASP ASVS 4.0.3:
    //   V5.3.9 → برنامه باید در برابر حملات Local File Inclusion (LFI) و
    //            Remote File Inclusion (RFI) محافظت کند
    // ------------------------------------------------------------------------
    [HttpGet]
    public IActionResult PathTraversal(string? input)
    {
        const string asvsRef = "V5.3.9";
        const string summary = "برنامه باید جلوی دسترسی به فایل‌های خارج از مسیر مجاز (با ../ یا \\..\\) را بگیرد.";

        if (string.IsNullOrEmpty(input))
        {
            return View("../Shared/DemoForm", new DemoFormViewModel
            {
                Title = "حمله Path Traversal (خواندن فایل غیرمجاز)",
                AsvsRef = asvsRef,
                AsvsSummary = summary,
                Description = "فایل‌های عمومی در پوشه DemoFiles پروژه قرار دارند (public-note.txt). مهاجم با الگوی ../ می‌تواند از پوشه خارج شود و فایل محرمانه secret.txt (که شبیه‌سازی کلیدهای سرویس است) را بخواند. این الگو را امتحان کنید: ../secret.txt",
                InputLabel = "نام فایل",
                FormAction = "/InputValidation/PathTraversal",
                PresetValue = "../secret.txt"
            });
        }

        var result = new DemoResult
        {
            Title = "حمله Path Traversal (خواندن فایل غیرمجاز)",
            AsvsRef = asvsRef,
            AsvsSummary = summary,
            Explanation = "اگر نام فایل دریافتی از کاربر را بدون بررسی با Path.Combine به مسیر ریشه بچسبانیم، ورودی‌هایی مثل ../../secret.txt از پوشه مجاز خارج می‌شوند و هر فایلی از دیسک خوانده می‌شود (Web.Config، فایل‌های سیستم و...). راه‌حل استاندارد: پس از ساخت مسیر نهایی با Path.GetFullPath، بررسی کنیم که مسیر نهایی هنوز داخل پوشه مجاز (Web Root) است؛ به این تکنیک «Containment Check» می‌گویند.",
            CodeSnippet = """
                // ❌ آسیب‌پذیر:
                var path = Path.Combine(demoRoot, userInput); // ../../secret.txt فرار می‌کند!

                // ✓ امن: بررسی محصور بودن مسیر (بند V5.3.9)
                var fullPath = Path.GetFullPath(Path.Combine(demoRoot, userInput));
                if (!fullPath.StartsWith(demoRoot))
                    return NotFound("مسیر غیرمجاز است");

                // لایه دفاعی دوم: اعتبارسنجی allow-list نام فایل (بند V5.1.3)
                if (!Regex.IsMatch(userInput, @"^[a-zA-Z0-9._-]+$"))
                    return BadRequest("نام فایل فقط حرف/عدد/نقطه/خط تیره");
                """,
            BackController = "InputValidation"
        };

        // پوشه مجاز: <پروژه>/DemoFiles — و فایل محرمانه بیرون آن (<پروژه>/secret.txt)
        string webRoot = Directory.GetCurrentDirectory();
        string demoRoot = Path.Combine(webRoot, "DemoFiles");
        string secretFile = Path.Combine(webRoot, "secret.txt"); // شبیه‌سازی فایل حساس بیرون پوشه مجاز
        if (!System.IO.File.Exists(secretFile))
            System.IO.File.WriteAllText(secretFile, "این یک «فایل محرمانه» شبیه‌سازی‌شده است: API_KEY=sk-XXXX-TEST-12345");

        result.OutputLines.Add($"ورودی کاربر: {input}");
        result.OutputLines.Add("");

        // ------------------- بخش ۱: نسخه آسیب‌پذیر -------------------
        result.OutputLines.Add("【۱】 نسخه آسیب‌پذیر (بدون بررسی محدوده):");
        string vulnerablePath = Path.Combine(demoRoot, input);
        result.OutputLines.Add($"   مسیر ساخته‌شده: {vulnerablePath}");
        if (System.IO.File.Exists(vulnerablePath))
        {
            string content = System.IO.File.ReadAllText(vulnerablePath);
            result.OutputLines.Add($"   🔴 فایل خوانده شد! محتوا: {content}");
            bool escapedRoot = !Path.GetFullPath(vulnerablePath).StartsWith(demoRoot);
            result.OutputLines.Add(escapedRoot
                ? "   🔴 حمله موفق بود! مهاجم از پوشه مجاز خارج شد و فایل محرمانه را خواند."
                : "   (این فایل داخل پوشه مجاز بود)");
        }
        else
        {
            result.OutputLines.Add("   این مسیر در این سیستم وجود ندارد (اما در دنیای واقعی می‌توانست Web.Config باشد!).");
        }

        result.OutputLines.Add("");

        // ------------------- بخش ۲: نسخه امن -------------------
        result.OutputLines.Add("【۲】 نسخه امن (بررسی محصور بودن مسیر + Allow-List):");
        string safeFullPath = Path.GetFullPath(Path.Combine(demoRoot, input));
        bool contained = safeFullPath.StartsWith(demoRoot + Path.DirectorySeparatorChar);
        bool nameValid = System.Text.RegularExpressions.Regex.IsMatch(input, @"^[a-zA-Z0-9._-]+$");

        result.OutputLines.Add($"   مسیر نهایی: {safeFullPath}");
        result.OutputLines.Add($"   بررسی محصور بودن در پوشه مجاز: {(contained ? "عبور ✓" : "رد ⛔ (تلاش برای فرار از پوشه)")}");
        result.OutputLines.Add($"   بررسی Allow-List نام فایل: {(nameValid ? "عبور ✓" : "رد ⛔ (کاراکتر غیرمجاز در نام)")}");

        if (contained && nameValid && System.IO.File.Exists(safeFullPath))
        {
            result.OutputLines.Add($"   ✓ فایل مجاز خوانده شد: {System.IO.File.ReadAllText(safeFullPath)}");
        }
        else
        {
            result.OutputLines.Add("   ⛔ درخواست رد شد (HTTP 400/404) و حمله بی‌اثر بود ✓");
        }

        return View("../Shared/DemoResult", result);
    }

    // ------------------------------------------------------------------------
    // 🎯 دمو ۴ (فرم ورودی): اعتبارسنجی با Allow-List (لیست سفید)
    //
    // ⚙️ بندهای استاندارد OWASP ASVS 4.0.3:
    //   V5.1.3 → همه ورودی‌ها با اعتبارسنجی مثبت (Allow-List) بررسی شوند
    //   V5.1.4 → داده‌های ساختارمند باید با نوع قوی و الگوی تعریف‌شده اعتبارسنجی شوند
    // ------------------------------------------------------------------------
    [HttpGet]
    public IActionResult AllowList(string? input)
    {
        const string asvsRef = "V5.1.3 — V5.1.4";
        const string summary = "ورودی باید با اعتبارسنجی مثبت (تعیین اینکه چه چیزی مجاز است، نه چه چیزی ممنوع) و با نوع قوی/الگوی مشخص بررسی شود.";

        if (string.IsNullOrEmpty(input))
        {
            return View("../Shared/DemoForm", new DemoFormViewModel
            {
                Title = "اعتبارسنجی Allow-List روی ورودی",
                AsvsRef = asvsRef,
                AsvsSummary = summary,
                Description = "در این دمو فیلد «کد ملی» کاربر با الگوی سخت‌گیرانه (فقط ۱۰ رقم بدون کاراکتر اضافه) اعتبارسنجی می‌شود. هر ورودیِ غیرمجاز — حتی تزریق کد — با این الگو رد می‌شود. مقدار معتبر: 0012345678",
                InputLabel = "کد ملی",
                FormAction = "/InputValidation/AllowList",
                PresetValue = "0012345678"
            });
        }

        var result = new DemoResult
        {
            Title = "اعتبارسنجی Allow-List روی ورودی",
            AsvsRef = asvsRef,
            AsvsSummary = summary,
            Explanation = "اعتبارسنجی منفی («این کاراکترها را بردار») همیشه یک قدم عقب‌تر از مهاجم است، چون لیست حملات بی‌پایان است. اعتبارسنجی مثبت می‌گوید «فقط این فرمتِ مشخص، فقط این طول، فقط این کاراکترها قبول است». الگوی سخت‌گیرانه ۱۰ رقم، هر تزریقی را از اساس ناممکن می‌کند؛ چون هیچ کاراکتری غیر از رقم اصلاً وارد سیستم نمی‌شود.",
            CodeSnippet = """
                // اعتبارسنجی مثبت با الگوی سخت‌گیرانه (بندهای V5.1.3 و V5.1.4)
                if (!Regex.IsMatch(nationalId, @"^\d{10}$"))
                {
                    ModelState.AddModelError("", "کد ملی باید دقیقاً ۱۰ رقم باشد");
                    return BadRequest(ModelState);
                }

                // در Model با Data Annotations هم می‌توان:
                // [RegularExpression(@"^\d{10}$")]
                """,
            BackController = "InputValidation"
        };

        bool isValid = System.Text.RegularExpressions.Regex.IsMatch(input, @"^\d{10}$");

        result.OutputLines.Add($"ورودی: {input}");
        result.OutputLines.Add($"الگوی مجاز: دقیقاً ۱۰ رقم (بدون فاصله، خط تیره یا هر کاراکتر دیگر)");
        result.OutputLines.Add(new string('-', 50));
        result.OutputLines.Add($"نتیجه اعتبارسنجی: {(isValid ? "✓ معتبر — ورودی پذیرفته شد" : "⛔ نامعتبر — ورودی رد شد (HTTP 400)")}");
        result.OutputLines.Add("");

        if (!isValid)
        {
            result.OutputLines.Add("تفکیک دلیل رد شدن:");
            if (input.Length != 10) result.OutputLines.Add($"   ✗ طول ورودی {input.Length} کاراکتر است (باید دقیقاً ۱۰ باشد)");
            if (input.Any(c => !char.IsDigit(c)))
            {
                var badChars = input.Where(c => !char.IsDigit(c)).Distinct().ToList();
                result.OutputLines.Add($"   ✗ این کاراکترها غیررقمی‌اند: {string.Join(" ", badChars)}");
            }
        }
        result.OutputLines.Add("");
        result.OutputLines.Add("✓ با این الگو، هر ورودی خطرناک (تگ، کوتیشن، دستور SQL و...) اصلاً وارد برنامه نمی‌شود.");

        return View("../Shared/DemoResult", result);
    }
}
