// ============================================================================
//  MethodInjectionDemo.cs — نمایش تزریق از طریق پارامتر متد (Method Injection)
// ----------------------------------------------------------------------------
//  در Method Injection، وابستگی به‌صورت پارامتر یک متد به کلاس داده می‌شود.
//  کاربردها:
//   - وقتی وابستگی فقط در یک متد خاص نیاز است (نه در طول عمر کلاس).
//   - وقتی می‌خواهیم وابستگی را در هر بار فراخوانی متد کنترل کنیم.
//   - در Action Methodهای MVC، می‌توانیم سرویس را به‌عنوان پارامتر [FromServices] بگیریم.
//  مزایا:
//   - وابستگی اختیاری است؛ اگر نیازی نبود، اصلاً تزریق نمی‌شود.
//   - انعطاف‌پذیری بالا.
//  معایب:
//   - متد به وابستگی خارجی گره می‌خورد (کمی تست‌پذیری کاهش می‌یابد).
// ============================================================================

namespace DIEduDemo.Services;

/// <summary>
/// این کلاس، یک سرویس IOperationScoped را در Constructor نمی‌گیرد،
/// بلکه در متد ProcessIt به‌عنوان پارامتر دریافت می‌کند.
/// </summary>
public class MethodInjectionDemo
{
    private readonly ILogger<MethodInjectionDemo> _logger;

    /// <summary>
    /// در Constructor فقط ILogger می‌گیریم (سرویس داخلی)
    /// </summary>
    public MethodInjectionDemo(ILogger<MethodInjectionDemo> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// تزریق در سطح متد: IOperationScoped به‌عنوان پارامتر به متد داده می‌شود.
    /// این الگو برای زمانی مناسب است که سرویس فقط در یک عملیات خاص نیاز است.
    /// </summary>
    /// <param name="operation">سرویس Scoped تزریق‌شده</param>
    /// <returns>پیام نمایشی همراه با Guid سرویس</returns>
    public string ProcessIt(IOperationScoped operation)
    {
        _logger.LogInformation("ProcessIt فراخوانی شد با ScopedGuid={Guid}", operation.OperationId);
        return $"Method Injection: Guid سرویس Scoped = {operation.OperationId}";
    }

    /// <summary>
    /// تزریق چند وابستگی به یک متد (نمونه با دو پارامتر)
    /// </summary>
    public string ProcessMultiple(IOperationTransient transient, IOperationSingleton singleton)
    {
        return $"Method Injection (Multi): Transient={transient.OperationId}, Singleton={singleton.OperationId}";
    }
}
