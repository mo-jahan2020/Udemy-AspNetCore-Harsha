using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using OllamaSK.Api.Models;
using OllamaSK.Api.Plugins;

namespace OllamaSK.Api.Controllers;

/// <summary>
/// کنترلر ابزارها — فراخوانی مستقیم Plugin‌های ثبت‌شده در Semantic Kernel
/// (بدون دخالت مدل) برای تست و اطمینان از صحت کار آن‌ها.
/// </summary>
[ApiController]
[Route("api/tools")]
public class ToolsController(Kernel kernel, SearchPlugin search) : ControllerBase
{
    /// <summary>GET /api/tools → لیست همه Plugin‌ها و توابع ثبت‌شده در Kernel</summary>
    [HttpGet]
    public IActionResult List()
    {
        var tools = kernel.Plugins
            .SelectMany(p => p.Select(f => new { plugin = p.Name, function = f.Name, description = f.Description }))
            .ToList();

        return Ok(tools);
    }

    /// <summary>POST /api/tools/calculate → فراخوانی مستقیم Plugin ماشین‌حساب</summary>
    [HttpPost("calculate")]
    public async Task<IActionResult> Calculate([FromBody] CalcRequest request, CancellationToken ct)
    {
        // نام عملیات → نام تابع داخل CalculatorPlugin
        var funcName = (request.Operation?.Trim().ToLowerInvariant()) switch
        {
            "add" => "Add",
            "subtract" => "Subtract",
            "multiply" => "Multiply",
            "divide" => "Divide",
            "power" => "Power",
            _ => null
        };

        if (funcName is null)
            return BadRequest(new { error = "عملیات نامعتبر است. مقادیر مجاز: add، subtract، multiply، divide، power" });

        if (!kernel.Plugins.TryGetFunction("Calculator", funcName, out var function) || function is null)
            return NotFound(new { error = "Plugin ماشین‌حساب در Kernel ثبت نشده است." });

        // فراخوانی واقعی تابع از طریق موتور Semantic Kernel
        var args = new KernelArguments { ["a"] = request.A, ["b"] = request.B };
        var result = await kernel.InvokeAsync(function, args, ct);

        return Ok(new
        {
            plugin = "Calculator",
            function = funcName,
            arguments = new { a = request.A, b = request.B },
            result = result.GetValue<double>()
        });
    }

    /// <summary>GET /api/tools/search?q=... → فراخوانی مستقیم Plugin جستجوی وب (ارائه‌دهنده اختیاری: Parsijoo / Wikipedia / DuckDuckGo / Auto)</summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] string? provider = null, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new { error = "پارامتر q الزامی است." });

        var results = await search.WebSearch(q, provider, ct);
        return Ok(new { query = q, provider = provider ?? "Auto", results });
    }
}
