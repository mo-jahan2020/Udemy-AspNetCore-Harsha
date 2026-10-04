using Microsoft.AspNetCore.Mvc;
using OllamaSK.Api.Models;
using OllamaSK.Api.Services;

namespace OllamaSK.Api.Controllers;

/// <summary>
/// کنترلر مکالمات — تاریخچه پایا در SQLite:
/// لیست، ساخت دستی، مشاهده پیام‌ها، تغییر عنوان و حذف.
/// ذخیره خودکار پیام‌ها داخل ChatService انجام می‌شود.
/// </summary>
[ApiController]
[Route("api/conversations")]
public class ConversationsController(IConversationStore store) : ControllerBase
{
    /// <summary>GET /api/conversations → لیست همه مکالمات (آخرین فعالیت اول)</summary>
    [HttpGet]
    public IActionResult List()
        => Ok(store.GetConversations());

    /// <summary>POST /api/conversations → ساخت مکالمه خالی جدید (تیتر اختیاری)</summary>
    [HttpPost]
    public IActionResult Create([FromBody] CreateConversationRequest? request)
        => Ok(store.EnsureConversation(null, request?.Title ?? "مکالمه جدید"));

    /// <summary>GET /api/conversations/{id}/messages → پیام‌های یک مکالمه</summary>
    [HttpGet("{id:guid}/messages")]
    public IActionResult Messages(Guid id, [FromQuery] int limit = 200)
        => Ok(store.GetMessages(id, Math.Clamp(limit, 1, 500), int.MaxValue));

    /// <summary>PATCH /api/conversations/{id} → تغییر عنوان</summary>
    [HttpPatch("{id:guid}")]
    public IActionResult Rename(Guid id, [FromBody] RenameConversationRequest request)
        => store.Rename(id, request.Title)
            ? Ok(new { renamed = id, title = request.Title })
            : NotFound(new { error = "مکالمه پیدا نشد." });

    /// <summary>DELETE /api/conversations/{id} → حذف یک مکالمه و پیام‌هایش</summary>
    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
        => store.Delete(id)
            ? Ok(new { deleted = id })
            : NotFound(new { error = "مکالمه پیدا نشد." });

    /// <summary>DELETE /api/conversations → حذف همه مکالمات</summary>
    [HttpDelete]
    public IActionResult DeleteAll()
    {
        store.DeleteAll();
        return Ok(new { deleted = "all" });
    }
}
