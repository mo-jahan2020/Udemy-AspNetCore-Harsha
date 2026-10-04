using Microsoft.AspNetCore.Mvc;
using TextToSqlApi.Models.DTOs;
using TextToSqlApi.Services;

namespace TextToSqlApi.Controllers;

/// <summary>
/// کنترلر اصلی تولید SQL از پرامپت زبانی.
/// </summary>
[ApiController]
[Route("api/texttosql")]
[Produces("application/json")]
public class TextToSqlController : ControllerBase
{
    private readonly ITextToSqlService _textToSqlService;
    private readonly ISqlService _sqlService;
    private readonly ILogger<TextToSqlController> _logger;

    /// <summary>سازنده با تزریق سرویس‌ها.</summary>
    public TextToSqlController(
        ITextToSqlService textToSqlService,
        ISqlService sqlService,
        ILogger<TextToSqlController> logger)
    {
        _textToSqlService = textToSqlService;
        _sqlService = sqlService;
        _logger = logger;
    }

    /// <summary>تولید SQL از پرامپت زبانی کاربر.</summary>
    /// <param name="req">شامل SessionId و Prompt.</param>
    /// <param name="ct">Token لغو.</param>
    /// <returns>SQL تولیدشده به همراه متادیتا.</returns>
    /// <response code="200">SQL با موفقیت تولید شد.</response>
    /// <response code="400">ورودی نامعتبر.</response>
    [HttpPost("generate")]
    [ProducesResponseType(typeof(GenerateSqlResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GenerateSqlResponse>> Generate([FromBody] GenerateSqlRequest req, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _textToSqlService.GenerateAsync(req, ct);
        return Ok(result);
    }

    /// <summary>ثبت یا به‌روزرسانی شما دیتابیس برای آموزش Vanna.</summary>
    /// <param name="req">شامل SchemaText.</param>
    /// <param name="ct">Token لغو.</param>
    [HttpPost("schema")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateSchema([FromBody] UpdateSchemaRequest req, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await _textToSqlService.UpdateSchemaAsync(req, ct);
        return Ok(new { message = "Schema updated successfully." });
    }

    /// <summary>
    /// اجرای SQL تولیدشده فقط در حالت Read-Only.
    /// دستورات مخرب (DROP, DELETE, UPDATE, TRUNCATE, ALTER و ...) بلاک می‌شوند.
    /// </summary>
    /// <param name="req">شامل Sql و MaxRows.</param>
    /// <param name="ct">Token لغو.</param>
    /// <returns>نتایج اجرای SQL.</returns>
    [HttpPost("execute")]
    [ProducesResponseType(typeof(ExecuteSqlResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ExecuteSqlResponse>> Execute([FromBody] ExecuteSqlRequest req, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _sqlService.ExecuteReadOnlyAsync(req.Sql, req.MaxRows, ct);
        return Ok(result);
    }
}
