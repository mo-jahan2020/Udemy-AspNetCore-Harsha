using Microsoft.AspNetCore.Mvc;
using OllamaSK.Api.Models;
using OllamaSK.Api.Services;

namespace OllamaSK.Api.Controllers;

/// <summary>
/// کنترلر مدیریت فایل‌های پیوست (متنی / ورد / اکسل).
/// بعد از آپلود، سند به‌طور خودکار برای RAG ایندکس می‌شود (اگر مدل Embedding در دسترس باشد).
/// </summary>
[ApiController]
[Route("api/files")]
public class FilesController(IDocumentStore store, IDocumentParser parser, IRagService rag) : ControllerBase
{
    /// <summary>
    /// POST /api/files/upload → آپلود فایل (multipart/form-data، فیلد file).
    /// متن فایل استخراج، کوتاه‌سازی و در حافظه ذخیره می‌شود.
    /// </summary>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> Upload([FromForm] IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { error = "فایلی ارسال نشده است؛ فیلد «file» را در فرم پر کنید." });

        try
        {
            var content = await parser.ExtractAsync(file, ct);

            var doc = new UploadedDocument
            {
                Id = Guid.NewGuid(),
                FileName = file.FileName,
                FileKind = parser.DetectKind(file.FileName),
                SizeBytes = file.Length,
                Content = content,
                ContentChars = content.Length,
                UploadedAt = DateTimeOffset.UtcNow
            };
            store.Add(doc);

            // ایندکس معنایی (RAG) — اگر مدل Embedding نباشد، آپلود همچنان موفق می‌ماند
            var (indexed, chunkCount, ragNote) = await rag.IndexDocumentAsync(doc, ct);

            return Ok(new
            {
                doc.Id,
                doc.FileName,
                doc.FileKind,
                doc.ContentChars,
                ragIndexed = indexed,
                chunkCount,
                ragNote,
                preview = content[..Math.Min(300, content.Length)]
            });
        }
        catch (NotSupportedException ex) { return BadRequest(new { error = ex.Message }); }
        catch (InvalidDataException ex) { return BadRequest(new { error = ex.Message }); }
        catch (Exception ex) { return StatusCode(500, new { error = "خطا در پردازش فایل: " + ex.Message }); }
    }

    /// <summary>GET /api/files → لیست همه اسناد آپلودشده</summary>
    [HttpGet]
    public IActionResult List()
        => Ok(store.GetAll().Select(d => new
        {
            d.Id,
            d.FileName,
            d.FileKind,
            d.SizeBytes,
            d.ContentChars,
            d.UploadedAt
        }));

    /// <summary>GET /api/files/{id} → جزئیات یک سند (با پیش‌نمایش ۲۰۰۰ کاراکتری متن)</summary>
    [HttpGet("{id:guid}")]
    public IActionResult Get(Guid id)
    {
        var doc = store.Get(id);
        if (doc is null)
            return NotFound(new { error = "سند پیدا نشد." });

        return Ok(new
        {
            doc.Id,
            doc.FileName,
            doc.FileKind,
            doc.SizeBytes,
            doc.ContentChars,
            doc.UploadedAt,
            preview = doc.Content[..Math.Min(2000, doc.Content.Length)]
        });
    }

    /// <summary>DELETE /api/files/{id} → حذف سند</summary>
    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
        => store.Remove(id)
            ? Ok(new { deleted = id })
            : NotFound(new { error = "سند پیدا نشد." });
}
