using AsvsLab.Services;
using Microsoft.AspNetCore.Mvc;

namespace AsvsLab.Controllers;

// ═══════════════════════════════════════════════════════════════════
//  فصل V15 — کدنویسی امن و معماری (Secure Coding and Architecture)
//  الزامات این فصل بیشتر فرآیندی/معماری‌اند؛ طبق خواسته کاربر متنی ارائه شده‌اند
// ═══════════════════════════════════════════════════════════════════
public class V15Controller : DemoBaseController
{
    public V15Controller(AsvsRefService asvs, ProgressService progress) : base(asvs, progress) { }

    public IActionResult Index() =>
        SectionIndex(15, "معماری و کدنویسی امن: SBOM، مدیریت وابستگی‌ها، Mass Assignment و Prototype Pollution. دموهای این فصل ماهیتاً فرآیندی‌اند و متنی ارائه می‌شوند.");

    public IActionResult Sbom() =>
        View("../Shared/DemoResult", TextDemo("v15-sbom", "V15.1.2",
            "کاتالوگ موجودی نرم‌افزار (SBOM) از همه وابستگی‌ها نگهداری و از مخازن مورد اعتماد تأمین شود.", "V15",
            "تولید SBOM در همین پروژه (اجرای واقعی در خط فرمان شما):\n" +
            "   dotnet CycloneDX .\n" +
            "خروجی: bom.json حاوی همه وابستگی‌ها (Microsoft.Data.Sqlite، StackExchange.Redis و...)\n\n" +
            "نکته این پروژه: Bootstrap و فونت آفلاین شدند (wwwroot/lib) تا هیچ CDN ثالثی در زنجیره تأمین نباشد — " +
            "اگر CDN می‌خواهید، حتماً SRI اضافه کنید:\n" +
            "<script src=\"https://cdn.../x.js\" integrity=\"sha384-...\" crossorigin=\"anonymous\"></script>\n\n" +
            "مقابله با Dependency Confusion: پکیج‌های داخلی را در registry خصوصی با نام یکتا publish کنید (بند V15.2.4)."));

    public IActionResult Dependencies() =>
        View("../Shared/DemoResult", TextDemo("v15-deps", "V15.1.1,V15.2.1",
            "زمان‌بندی به‌روزرسانی وابستگی‌ها مستند و نسخه‌های منقضی/آسیب‌پذیر حذف شوند.", "V15",
            "اجرای واقعی روی همین پروژه:\n" +
            "   dotnet list package --vulnerable --include-transitive\n" +
            "این دستور NuGet.org و GHSA را برای همه وابستگی‌ها (حتی ترانزیتیو) چک می‌کند.\n\n" +
            "خط پایپ‌لاین CI:\n" +
            "   - run: dotnet list package --vulnerable --include-transitive && exit 1 || exit 0\n" +
            "(بیلد روی وابستگی آسیب‌پذیر بشکند)\n\n" +
            "سیاست نمونه (بند V15.1.1): آسیب‌پذیری بحرانی ← ۷ روز؛ بالا ← ۳۰ روز؛ بقیه ← ۹۰ روز.\n" +
            "ابزارهای تکمیلی: Dependabot/Renovate برای PR خودکار ارتقای نسخه."));

    public IActionResult MassAssignment() =>
        View("../Shared/DemoResult", TextDemo("v15-mass", "V15.3.3",
            "با محدود کردن فیلدهای قابل اتصال (binding) جلوی Mass Assignment گرفته شود.", "V15",
            "سناریوی حمله کلاسیک:\n\n" +
            "public class UserDto { public string Name; public bool IsAdmin; }  ← مدل واحد برای همه!\n" +
            "// مهاجم در فرم پروفایل، فیلد پنهان می‌فرستد:\n" +
            "// name=Ali&isAdmin=true  → خودش را ادمین کرد!\n\n" +
            "الگوهای دفاعی در ASP.NET Core:\n" +
            "۱) DTO جدا برای ورودی (بدون فیلد IsAdmin):\n" +
            "   public record UpdateProfileDto(string Name);\n" +
            "۲) [Bind(nameof(...))] روی اکشن:\n" +
            "   public IActionResult Update([Bind(\"Name\")] UserDto dto)\n" +
            "۳) TryUpdateModelAsync با فهرست فیلدهای مجاز\n\n" +
            "دموی اجرایی به endpoint ثبت کاربر «واقعی با فرم HTML جعلی» نیاز دارد؛ الگو + توضیح کافی است."));

    public IActionResult PrototypePollution() =>
        View("../Shared/DemoResult", TextDemo("v15-proto", "V15.3.6",
            "کد جاوااسکریپت به‌گونه‌ای نوشته شود که آلودگی prototype ممکن نباشد (مثلاً Set/Map).", "V15",
            "حمله در یک خط:\n" +
            "JSON.parse('{\"__proto__\": {\"isAdmin\": true}}')\n" +
            "// با merge ناامن: Object.assign(config, input) → همه آبجکت‌ها isAdmin می‌گیرند!\n\n" +
            "سه دفاع اصلی:\n" +
            "۱) عدم merge عمیق با ورودی کاربر؛ کلیدهای __proto__/constructor را فیلتر کنید\n" +
            "۲) استفاده از new Set()/new Map() به‌جای آبجکت معمولی (بند صراحتاً این را پیشنهاد می‌کند)\n" +
            "۳) Object.create(null) برای دیکشنری‌های بی‌prototype\n\n" +
            "تأثیر در بک‌اند: اگر کد JS سمت سرور (Node/SSR) با JSON ورودی merge کند، auth bypass کل برنامه ممکن است؛ " +
            "در ASP.NET Core سمت سرور سی‌شارپ است و prototype ندارد — اما کتابخانه‌های JS سمت کلاینت شما همچنان هدف‌اند."));
}
