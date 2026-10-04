using Microsoft.SemanticKernel;

namespace OllamaSK.Api.Filters;

/// <summary>
/// لاگ کردن فراخوانی Plugin‌ها در کنسول — نشان می‌دهد مدل کدام ابزار را
/// با چه آرگومان‌هایی صدا زده و چه نتیجه‌ای گرفته است.
/// این فیلتر توسط Semantic Kernel از DI برداشته می‌شود.
/// </summary>
public sealed class PluginLoggingFilter(ILogger<PluginLoggingFilter> logger) : IFunctionInvocationFilter
{
    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        var name = $"{context.Function.PluginName}.{context.Function.Name}";
        var args = string.Join(", ", context.Arguments.Select(kv => $"{kv.Key}={kv.Value}"));

        logger.LogInformation("→ فراخوانی Plugin: {Name} ({Args})", name, args);

        await next(context);

        logger.LogInformation("← نتیجه {Name}: {Result}", name, context.Result?.GetValue<object?>());
    }
}
