// ============================================================================
//  ConstructorInjectionDemo.cs — نمایش تزریق از طریق Constructor
// ----------------------------------------------------------------------------
//  Constructor Injection رایج‌ترین نوع تزریق وابستگی در ASP.NET Core است.
//  وابستگی‌ها به‌صورت پارامتر Constructor به کلاس داده می‌شوند.
//  مزایا:
//   - وابستگی‌ها به‌صراحت در امضای Constructor مشخص می‌شوند.
//   - اگر وابستگی‌ای ثبت نشده باشد، Container هنگام ساخت شیء خطا می‌دهد.
//   - فیلدها می‌توانند readonly باشند (Immutable).
//  معایب:
//   - اگر تعداد وابستگی‌ها زیاد باشد، ممکن است نشانه‌ای از نقض Single Responsibility باشد.
// ============================================================================

namespace DIEduDemo.Services;

/// <summary>
/// این کلاس، هر سه نوع سرویس IOperation را از طریق Constructor دریافت می‌کند.
/// این بهترین روش برای نشان دادن این است که یک کلاس می‌تواند چندین وابستگی داشته باشد.
/// </summary>
public class ConstructorInjectionDemo
{
    // تمام وابستگی‌ها به‌صورت readonly نگه‌داری می‌شوند تا Immutable باشند
    private readonly IOperationTransient _transient;
    private readonly IOperationScoped _scoped;
    private readonly IOperationSingleton _singleton;

    // سرویس داخلی ASP.NET Core: ILogger<T> برای لاگ‌گیری
    private readonly ILogger<ConstructorInjectionDemo> _logger;

    /// <summary>
    /// Constructor Injection: تمام وابستگی‌ها به‌عنوان پارامتر Constructor تزریق می‌شوند.
    /// Container هنگام ساخت این شیء، نمونه‌های مناسب را به‌صورت خودکار فراهم می‌کند.
    /// </summary>
    public ConstructorInjectionDemo(
        IOperationTransient transient,
        IOperationScoped scoped,
        IOperationSingleton singleton,
        ILogger<ConstructorInjectionDemo> logger)
    {
        _transient = transient;
        _scoped = scoped;
        _singleton = singleton;
        _logger = logger;

        // نمونه‌ای از استفاده سرویس داخلی ILogger<T>
        _logger.LogInformation("ConstructorInjectionDemo ساخته شد. SingletonGuid={SingletonGuid}", singleton.OperationId);
    }

    /// <summary>
    /// خروجی برای نمایش در View — شامل Guid هر سه سرویس
    /// </summary>
    public (Guid Transient, Guid Scoped, Guid Singleton) GetCurrentIds() =>
        (_transient.OperationId, _scoped.OperationId, _singleton.OperationId);
}
