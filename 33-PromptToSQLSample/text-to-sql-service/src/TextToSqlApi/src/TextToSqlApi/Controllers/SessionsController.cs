using Microsoft.AspNetCore.Mvc;
using TextToSqlApi.Models.DTOs;
using TextToSqlApi.Services;

namespace TextToSqlApi.Controllers;

/// <summary>
/// کنترلر مدیریت نشست‌ها (Sessions) با Endpoint های کامل CRUD.
/// </summary>
[ApiController]
[Route("api/sessions")]
[Produces("application/json")]
public class SessionsController : ControllerBase
{
    private readonly ISessionService _sessionService;
    private readonly ILogger<SessionsController> _logger;

    /// <summary>سازنده با تزریق سرویس.</summary>
    public SessionsController(ISessionService sessionService, ILogger<SessionsController> logger)
    {
        _sessionService = sessionService;
        _logger = logger;
    }

    /// <summary>ایجاد یک نشست جدید.</summary>
    /// <param name="req">شامل UserName و Title اختیاری.</param>
    /// <param name="ct">Token لغو.</param>
    /// <response code="201">نشست با موفقیت ایجاد شد.</response>
    [HttpPost]
    [ProducesResponseType(typeof(SessionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SessionDto>> Create([FromBody] CreateSessionRequest req, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var session = await _sessionService.CreateSessionAsync(req, ct);
        return CreatedAtAction(nameof(GetHistory), new { id = session.Id }, session);
    }

    /// <summary>دریافت تاریخچه کامل یک نشست (شامل پیام‌ها).</summary>
    /// <param name="id">شناسه نشست.</param>
    /// <param name="ct">Token لغو.</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SessionHistoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SessionHistoryDto>> GetHistory(Guid id, CancellationToken ct)
    {
        var history = await _sessionService.GetSessionHistoryAsync(id, ct);
        if (history == null) return NotFound(new { message = $"Session {id} not found." });
        return Ok(history);
    }

    /// <summary>لیست نشست‌های یک کاربر.</summary>
    /// <param name="userName">نام کاربر (Query Parameter).</param>
    /// <param name="ct">Token لغو.</param>
    [HttpGet]
    [ProducesResponseType(typeof(List<SessionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<SessionDto>>> ListByUser([FromQuery] string userName, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userName))
            return BadRequest(new { message = "userName query parameter is required." });

        var list = await _sessionService.GetSessionsByUserAsync(userName, ct);
        return Ok(list);
    }

    /// <summary>حذف یک نشست (Cascade با پیام‌ها).</summary>
    /// <param name="id">شناسه نشست.</param>
    /// <param name="ct">Token لغو.</param>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var ok = await _sessionService.DeleteSessionAsync(id, ct);
        if (!ok) return NotFound(new { message = $"Session {id} not found." });
        return NoContent();
    }
}
