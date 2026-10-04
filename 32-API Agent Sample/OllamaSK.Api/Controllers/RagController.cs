using Microsoft.AspNetCore.Mvc;
using OllamaSK.Api.Models;
using OllamaSK.Api.Services;

namespace OllamaSK.Api.Controllers;

/// <summary>
/// کنترلر RAG — وضعیت زیرساخت وکتوری، جستجوی معنایی مستقیم و مدیریت ایندکس.
/// برای تست، سؤال را Embed می‌کنیم و نزدیک‌ترین قطعات اسناد را با امتیاز شباهت می‌بینیم.
/// </summary>
[ApiController]
[Route("api/rag")]
public class RagController(IRagService rag, IVectorStore vectors, IEmbeddingService embedding) : ControllerBase
{
    /// <summary>GET /api/rag/status → مدل Embedding، تعداد قطعات و اسناد ایندکس‌شده</summary>
    [HttpGet("status")]
    public async Task<IActionResult> Status(CancellationToken ct)
        => Ok(await rag.GetStatusAsync());

    /// <summary>
    /// GET /api/rag/search?q=... → جستجوی معنایی مستقیم در قطعات اسناد.
    /// پارامترهای اختیاری: topK (پیش‌فرض ۴) و docIds جدا شده با کاما.
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int topK = 4, [FromQuery] string? docIds = null, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new { error = "پارامتر q الزامی است." });

        if (!await embedding.IsModelAvailableAsync(ct))
            return StatusCode(503, new
            {
                error = $"مدل Embedding «{embedding.ModelId}» در دسترس نیست.",
                hint = $"در ترمینال بزنید: ollama pull {embedding.ModelId}"
            });

        var vector = (await embedding.EmbedAsync([q], ct)).FirstOrDefault();
        if (vector is null)
            return StatusCode(500, new { error = "تولید وکتور سؤال ناموفق بود." });

        Guid[]? filter = null;
        if (!string.IsNullOrWhiteSpace(docIds))
        {
            filter = docIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(part => Guid.TryParse(part, out _))
                .Select(Guid.Parse)
                .ToArray();
        }

        var hits = vectors.Search(vector, filter, Math.Clamp(topK, 1, 20));
        return Ok(new { query = q, count = hits.Count, results = hits });
    }

    /// <summary>POST /api/rag/reindex → ایندکس مجدد همه اسناد (مثلاً بعد از نصب مدل Embedding)</summary>
    [HttpPost("reindex")]
    public async Task<IActionResult> Reindex(CancellationToken ct)
    {
        if (vectors.Count() == 0)
            return BadRequest(new { error = "هیچ سندی برای ایندکس وجود ندارد؛ اول فایل آپلود کنید." });

        var (indexed, totalChunks, errors) = await rag.ReindexAllAsync(ct);
        return Ok(new { indexed, totalChunks, errors });
    }

    /// <summary>DELETE /api/rag/index/{docId} → حذف ایندکس وکتوری یک سند</summary>
    [HttpDelete("index/{docId:guid}")]
    public IActionResult DeleteIndex(Guid docId)
        => vectors.DeleteByDocument(docId)
            ? Ok(new { deleted = docId })
            : NotFound(new { error = "ایندکسی برای این سند پیدا نشد." });
}
