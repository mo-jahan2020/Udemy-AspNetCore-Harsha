// ============================================================================
//  LifetimeComparisonModel.cs — ViewModel برای نمایش مقایسه Lifetimeها
// ----------------------------------------------------------------------------
//  این مدل، تمام اطلاعاتی که در View خروجی نمایش می‌دهیم را در خود نگه می‌دارد.
// ============================================================================

namespace DIEduDemo.Models;

/// <summary>
/// هر ردیف از جدول مقایسه Lifetimeها با این کلاس نمایش داده می‌شود.
/// </summary>
public class LifetimeRow
{
    /// <summary>نام Lifetime (Transient / Scoped / Singleton)</summary>
    public string Lifetime { get; set; } = string.Empty;

    /// <summary>Guid اولین نمونه درخواست‌شده در این Request</summary>
    public Guid FirstCallGuid { get; set; }

    /// <summary>Guid دومین نمونه درخواست‌شده در همین Request</summary>
    public Guid SecondCallGuid { get; set; }

    /// <summary>آیا هر دو نمونه یکسان بودند؟ (در همین Request)</summary>
    public bool SameInRequest => FirstCallGuid == SecondCallGuid;

    /// <summary>توضیح فارسی رفتار این Lifetime</summary>
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// مدل نمایش هر سرویس Keyed در View
/// </summary>
public class KeyedServiceInfo
{
    /// <summary>کلید سرویس (مثلاً "email")</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>نام کانال (مثلاً "Email")</summary>
    public string ChannelName { get; set; } = string.Empty;

    /// <summary>نام کلاس پیاده‌سازی‌کننده</summary>
    public string ImplementationName { get; set; } = string.Empty;

    /// <summary>نتیجه فراخوانی Send با این سرویس</summary>
    public string SendResult { get; set; } = string.Empty;
}

/// <summary>
/// مدل نمایش محصول در View
/// </summary>
public class ProductViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// مدل کلی صفحه مقایسه Lifetimeها
/// </summary>
public class LifetimeComparisonModel
{
    /// <summary>ردیف‌های جدول مقایسه</summary>
    public List<LifetimeRow> Rows { get; set; } = new();

    /// <summary>نتیجه تزریق Constructor Injection</summary>
    public string ConstructorInjectionResult { get; set; } = string.Empty;

    /// <summary>نتیجه تزریق Method Injection</summary>
    public string MethodInjectionResult { get; set; } = string.Empty;

    /// <summary>نتیجه تزریق Property Injection</summary>
    public string PropertyInjectionResult { get; set; } = string.Empty;

    /// <summary>نتیجه فراخوانی سرویس بیزینسی IOrderProcessor</summary>
    public string OrderProcessorResult { get; set; } = string.Empty;

    /// <summary>نام محیط اجرا (Development/Staging/Production)</summary>
    public string EnvironmentName { get; set; } = string.Empty;

    /// <summary>زمان ثبت درخواست (برای نمایش)</summary>
    public DateTime RequestTime { get; set; }

    // =====================================================================
    // بخش‌های جدید: Keyed Services و DbContext
    // =====================================================================

    /// <summary>اطلاعات هر سه سرویس Keyed</summary>
    public List<KeyedServiceInfo> KeyedServices { get; set; } = new();

    /// <summary>لیست محصولات در دیتابیس</summary>
    public List<ProductViewModel> Products { get; set; } = new();

    /// <summary>تعداد کل محصولات</summary>
    public int ProductCount { get; set; }

    /// <summary>نام دیتابیس In-Memory</summary>
    public string DatabaseName { get; set; } = string.Empty;

    /// <summary>نام Provider دیتابیس</summary>
    public string DatabaseProvider { get; set; } = string.Empty;
}
