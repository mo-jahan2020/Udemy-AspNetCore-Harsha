namespace AsvsLab.Models;

// ============================================================================
// مدل نتیجه اجرای هر دمو (هر دکمه) — در ویوی مشترک DemoResult نمایش داده می‌شود
// ============================================================================
public class DemoResult
{
    /// <summary>عنوان فارسی دمو</summary>
    public string Title { get; set; } = "";

    /// <summary>شماره بندهای مرجع در استاندارد OWASP ASVS 4.0.3 (مثلاً V2.4.1)</summary>
    public string AsvsRef { get; set; } = "";

    /// <summary>خلاصه فارسی متن بندهای مرجع</summary>
    public string AsvsSummary { get; set; } = "";

    /// <summary>توضیح فارسی: این دمو چه می‌کند و چرا مهم است</summary>
    public string Explanation { get; set; } = "";

    /// <summary>قطعه کد اصلی دمو (برای نمایش آموزشی در صفحه نتیجه)</summary>
    public string CodeSnippet { get; set; } = "";

    /// <summary>خروجی واقعی اجرای دمو، خط به خط</summary>
    public List<string> OutputLines { get; set; } = new();

    /// <summary>آیا دمو با موفقیت اجرا شد؟</summary>
    public bool Success { get; set; } = true;

    /// <summary>اگر این دمو به سرویس جانبی (مانند Redis) نیاز دارد، توضیح آن</summary>
    public string? ServiceRequirement { get; set; }

    /// <summary>نام کنترلر برای دکمه بازگشت</summary>
    public string BackController { get; set; } = "Home";
}

// ============================================================================
// مدل فرم ورودی دموها (دموهایی که کاربر در آن‌ها مقدار وارد می‌کند)
// ============================================================================
public class DemoFormViewModel
{
    public string Title { get; set; } = "";
    public string AsvsRef { get; set; } = "";
    public string AsvsSummary { get; set; } = "";
    public string Description { get; set; } = "";
    public string InputLabel { get; set; } = "مقدار ورودی";
    public string FormAction { get; set; } = "";   // آدرس Action پردازش فرم
    public bool IsPassword { get; set; } = false;  // نمایش ورودی به صورت رمز عبور
    public string PresetValue { get; set; } = "";  // مقدار پیشنهادی پیش‌فرض
}

// ============================================================================
// مدل فهرست دموهای هر بخش (صفحه Index هر کنترلر)
// ============================================================================
public class DemoLink
{
    public string Action { get; set; } = "";
    public string Title { get; set; } = "";
    public string AsvsRef { get; set; } = "";
}

public class SectionCatalog
{
    public string Title { get; set; } = "";
    public string ChapterRef { get; set; } = "";
    public List<DemoLink> Demos { get; set; } = new();
}

// ============================================================================
// مدل یک فاکتور ساختگی برای دموی IDOR (بند V4.2.1)
// ============================================================================
public class Invoice
{
    public int Id { get; set; }
    public int OwnerUserId { get; set; }
    public string OwnerName { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Amount { get; set; }
}
