// ============================================================================
//  OrderProcessor.cs — پیاده‌سازی سرویس IOrderProcessor
// ----------------------------------------------------------------------------
//  این کلاس، یک سرویس Scoped است (در Program.cs با AddScoped ثبت شده).
//  به اینترفیس IOperationScoped وابسته است تا نشان دهد چطور یک سرویس می‌تواند
//  به سرویس‌های دیگر با Lifetime مناسب وابسته باشد.
// ============================================================================

namespace DIEduDemo.Services;

/// <summary>
/// پیاده‌سازی نمونه از IOrderProcessor.
/// در Constructor، IOperationScoped و ILogger را دریافت می‌کند.
/// </summary>
public class OrderProcessor : IOrderProcessor
{
    private readonly IOperationScoped _scopedOp;
    private readonly ILogger<OrderProcessor> _logger;

    /// <summary>
    /// Constructor Injection: دو وابستگی تزریق می‌شوند.
    /// </summary>
    public OrderProcessor(IOperationScoped scopedOp, ILogger<OrderProcessor> logger)
    {
        _scopedOp = scopedOp;
        _logger = logger;
    }

    /// <inheritdoc />
    public string ProcessOrder(int orderId)
    {
        _logger.LogInformation("ProcessOrder فراخوانی شد. OrderId={OrderId}", orderId);
        return $"سفارش #{orderId} با موفقیت پردازش شد. (Scoped OperationGuid: {_scopedOp.OperationId})";
    }
}
