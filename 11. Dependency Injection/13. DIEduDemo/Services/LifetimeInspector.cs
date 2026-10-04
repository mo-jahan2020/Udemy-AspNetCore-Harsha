// ============================================================================
//  LifetimeInspector.cs — سرویسی برای مقایسه هر سه Lifetime در یک لحظه
// ----------------------------------------------------------------------------
//  این سرویس هر سه نوع IOperation را در Constructor می‌گیرد.
//  در یک HTTP Request، اگر در دو نقطه‌ی مختلف کد این سرویس را درخواست کنیم:
//   - Transient: دو Guid متفاوت می‌بینیم
//   - Scoped:    دو Guid یکسان می‌بینیم (در همان Request)
//   - Singleton: در تمام Requestها یک Guid ثابت می‌بینیم
// ============================================================================

namespace DIEduDemo.Services;

/// <summary>
/// این سرویس برای نمایش تفاوت رفتار Lifetimeها طراحی شده است.
/// در Controller دو بار از این سرویس نمونه می‌گیریم تا تفاوت را در یک خروجی ببینید.
/// </summary>
public class LifetimeInspector
{
    public IOperationTransient Transient { get; }
    public IOperationScoped Scoped { get; }
    public IOperationSingleton Singleton { get; }

    /// <summary>
    /// Constructor Injection: هر سه وابستگی به‌صورت پارامتر دریافت می‌شوند.
    /// </summary>
    public LifetimeInspector(
        IOperationTransient transient,
        IOperationScoped scoped,
        IOperationSingleton singleton)
    {
        Transient = transient;
        Scoped = scoped;
        Singleton = singleton;
    }
}
