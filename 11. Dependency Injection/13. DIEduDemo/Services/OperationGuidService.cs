// ============================================================================
//  OperationGuidService.cs — پیاده‌سازی مشترک برای سه اینترفیس Lifetime
// ----------------------------------------------------------------------------
//  این کلاس، سه اینترفیس IOperationTransient / Scoped / Singleton را پیاده‌سازی می‌کند.
//  با این ترفند، یک منطق واحد داریم اما سه ثبت مجزا در Program.cs باعث می‌شود
//  Container برای هر Lifetime، نمونه‌ای با رفتار متفاوت بسازد.
// ============================================================================

namespace DIEduDemo.Services;

/// <summary>
/// پیاده‌سازی مشترک سه اینترفیس.
/// هر بار که یک نمونه از این کلاس ساخته می‌شود، یک Guid جدید در Constructor تولید می‌شود.
/// بنابراین اگر در خروجی OperationId برابر بود، یعنی همان نمونه قبلی استفاده شده است.
/// </summary>
public class OperationGuidService : IOperationTransient, IOperationScoped, IOperationSingleton
{
    //_guid در Constructor مقداردهی می‌شود تا هر نمونه یک شناسه یکتا داشته باشد
    private readonly Guid _operationId;

    /// <summary>
    /// Constructor پیش‌فرض: هر بار که Container یک نمونه می‌سازد، این متد اجرا می‌شود.
    /// </summary>
    public OperationGuidService()
    {
        _operationId = Guid.NewGuid();
    }

    /// <inheritdoc />
    public Guid OperationId => _operationId;

    /// <inheritdoc />
    public string LifetimeName => "OperationGuidService";
}
