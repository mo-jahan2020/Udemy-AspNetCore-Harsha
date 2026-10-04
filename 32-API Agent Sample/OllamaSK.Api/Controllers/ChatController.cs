using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using OllamaSK.Api.Models;
using OllamaSK.Api.Services;

namespace OllamaSK.Api.Controllers;

/// <summary>کنترلر چت — هم پاسخ کامل و هم استریم زنده (SSE)</summary>
[ApiController]
[Route("api/chat")]
public class ChatController(IChatService chatService) : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    /// <summary>POST /api/chat → پاسخ کامل به‌صورت JSON (بدون استریم)</summary>
    [HttpPost]
    public async Task<ActionResult<ChatResponse>> Ask([FromBody] ChatRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest(new { error = "متن پیام خالی است." });

        try
        {
            return Ok(await chatService.AskAsync(request, ct));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "خطا در ارتباط با مدل: " + ex.Message });
        }
    }

    /// <summary>
    /// POST /api/chat/stream → پاسخ استریم به‌صورت Server-Sent Events.
    /// هر event به شکل «data: {"type":"answer","content":"..."}» ارسال می‌شود.
    /// </summary>
    [HttpPost("stream")]
    public async Task Stream([FromBody] ChatRequest request, CancellationToken ct)
    {
        Response.StatusCode = 200;
        Response.ContentType = "text/event-stream; charset=utf-8";
        Response.Headers.CacheControl = "no-cache";

        if (string.IsNullOrWhiteSpace(request.Message))
        {
            await WriteSse(new StreamChunk("error", "متن پیام خالی است."), CancellationToken.None);
            await WriteSse(new StreamChunk("done", string.Empty), CancellationToken.None);
            return;
        }

        try
        {
            await foreach (var chunk in chatService.AskStreamAsync(request, ct))
            {
                await WriteSse(chunk, ct);
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // کلاینت اتصال را قطع کرده است؛ کافی است جریان را ببندیم
        }
        catch (Exception ex)
        {
            await WriteSse(new StreamChunk("error", "خطا در ارتباط با مدل: " + ex.Message), CancellationToken.None);
            await WriteSse(new StreamChunk("done", string.Empty), CancellationToken.None);
        }
    }

    private async Task WriteSse(StreamChunk chunk, CancellationToken token)
    {
        var json = JsonSerializer.Serialize(chunk, JsonOpts);
        await Response.WriteAsync($"data: {json}\n\n", Encoding.UTF8, token);
        await Response.Body.FlushAsync(token);
    }
}
