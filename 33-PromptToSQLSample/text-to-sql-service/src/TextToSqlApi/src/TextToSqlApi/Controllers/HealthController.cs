using Microsoft.AspNetCore.Mvc;
using TextToSqlApi.Models.DTOs;
using TextToSqlApi.Services;

namespace TextToSqlApi.Controllers;

/// <summary>
/// کنترلر Health Check برای پایش سرویس‌ها.
/// </summary>
[ApiController]
[Route("api/health")]
[Produces("application/json")]
public class HealthController : ControllerBase
{
    private readonly IOllamaService _ollama;
    private readonly IVannaService _vanna;
    private readonly ISqlService _sql;
    private readonly ILogger<HealthController> _logger;

    /// <summary>سازنده با تزریق سرویس‌ها.</summary>
    public HealthController(IOllamaService ollama, IVannaService vanna, ISqlService sql, ILogger<HealthController> logger)
    {
        _ollama = ollama;
        _vanna = vanna;
        _sql = sql;
        _logger = logger;
    }

    /// <summary>بررسی سلامت همه کامپوننت‌ها (Ollama, Vanna, SQL Server).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<HealthResponse>> Check(CancellationToken ct)
    {
        var ollamaHealthy = await _ollama.IsHealthyAsync(ct);
        var vannaHealthy = await _vanna.IsHealthyAsync(ct);
        var sqlHealthy = await _sql.IsHealthyAsync(ct);

        var allHealthy = ollamaHealthy && sqlHealthy; // Vanna اختیاری است

        var response = new HealthResponse
        {
            Status = allHealthy ? "Healthy" : "Degraded",
            Components =
            {
                ["Ollama"] = ollamaHealthy ? "Healthy" : "Unhealthy",
                ["Vanna"] = vannaHealthy ? "Healthy" : "Unhealthy",
                ["TargetDatabase"] = sqlHealthy ? "Healthy" : "Unhealthy"
            }
        };

        return Ok(response);
    }
}
