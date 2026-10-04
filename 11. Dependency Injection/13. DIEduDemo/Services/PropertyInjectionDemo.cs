// ============================================================================
//  PropertyInjectionDemo.cs — نمایش تزریق از طریق Property
// ----------------------------------------------------------------------------
//  Property Injection به‌صورت پیش‌فرض در Container داخلی ASP.NET Core پشتیبانی نمی‌شود.
//  برای فعال‌سازی آن، معمولاً از یکی از روش‌های زیر استفاده می‌کنیم:
//   1) استفاده از IServiceProvider و ساخت دستی شیء با ActivatorUtilities
//   2) استفاده از یک Factory در هنگام ثبت سرویس (AddTransient<T>(factory))
//   3) استفاده از کتابخانه‌های IoC جایگزین مثل Autofac یا Scrutor
//
//  در این مثال، روش ۲ را به‌کار برده‌ایم: در Program.cs، سرویس را با Factory ثبت کردیم
//  و داخل Factory، Property را با استفاده از IServiceProvider مقداردهی می‌کنیم.
//
//  مزایا:
//   - وابستگی‌های اختیاری (نه اجباری مثل Constructor)
//  معایب:
//   - کلاس به‌راحتی بدون وابستگی قابل ساخت است که ممکن است باعث NullReferenceException شود.
//   - Containerهای دیفالت ASP.NET Core آن را توصیه نمی‌کنند.
// ============================================================================

namespace DIEduDemo.Services;

/// <summary>
/// این کلاس نشان می‌دهد چگونه می‌توان یک وابستگی را به‌صورت Property تزریق کرد.
/// دقت کنید که Property، {get; set;} است تا Container بتواند آن را set کند.
/// </summary>
public class PropertyInjectionDemo
{
    /// <summary>
    /// این Property به‌صورت دستی توسط Factory در Program.cs تزریق می‌شود.
    /// </summary>
    public IOperationSingleton? SingletonOp { get; set; }

    /// <summary>
    /// این Property نیز به‌صورت دستی تزریق می‌شود.
    /// </summary>
    public ILogger<PropertyInjectionDemo>? Logger { get; set; }

    /// <summary>
    /// متد نمایشی برای خروجی
    /// </summary>
    public string Describe()
    {
        // چون Property ممکن است null باشد، باید بررسی کنیم
        if (SingletonOp is null)
            return "Property Injection: وابستگی تزریق نشده است!";

        Logger?.LogInformation("PropertyInjectionDemo.Describe فراخوانی شد");
        return $"Property Injection: Guid سرویس Singleton = {SingletonOp.OperationId}";
    }
}
