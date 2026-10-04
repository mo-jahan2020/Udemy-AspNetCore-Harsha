using System.Text;
using Microsoft.AspNetCore.Mvc;
using OllamaSharp;

namespace OllamaSK.Api.Controllers;

/// <summary>درخواست تولید متن مستقیم</summary>
public sealed record RawGenerateRequest(string Prompt);

/// <summary>
/// کنترلر Ollama — وضعیت سرویس و ارتباط مستقیم با OllamaSharp (بدون Semantic Kernel).
/// این کنترلر برای فهم بهتر مرحله ۶ است: اگر بخواهید فقط با پکیج OllamaSharp کار کنید،
/// دقیقاً همین سبک کد نوشته می‌شود؛ ولی در این پروژه چت اصلی از مسیر Semantic Kernel می‌رود.
/// </summary>
[ApiController]
[Route("api/ollama")]
public class OllamaController(IHttpClientFactory httpClientFactory, IConfiguration configuration) : ControllerBase
{
    /// <summary>GET /api/ollama/health → بررسی اتصال به Ollama و لیست مدل‌های نصب‌شده</summary>
    [HttpGet("health")]
    public async Task<IActionResult> Health(CancellationToken ct)
    {
        var client = httpClientFactory.CreateClient("ollama");

        try
        {
            var response = await client.GetAsync("/api/tags", ct);
            var json = await response.Content.ReadAsStringAsync(ct);

            // خروجی خام Ollama را با همان وضعیت HTTP برمی‌گردانیم
            return new ContentResult
            {
                Content = json,
                ContentType = "application/json",
                StatusCode = (int)response.StatusCode
            };
        }
        catch (Exception ex)
        {
            return StatusCode(503, new
            {
                error = "اتصال به Ollama برقرار نشد. مطمئن شوید Ollama اجرا است (ollama serve) و Endpoint درست است.",
                detail = ex.Message
            });
        }
    }

    /// <summary>POST /api/ollama/raw-generate → تولید متن با پکیج OllamaSharp (بدون Semantic Kernel)</summary>
    [HttpPost("raw-generate")]
    public async Task<IActionResult> RawGenerate([FromBody] RawGenerateRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Prompt))
            return BadRequest(new { error = "پارامتر prompt الزامی است." });

        var endpoint = configuration["Ollama:Endpoint"] ?? "http://localhost:11434";
        var modelId = configuration["Ollama:ModelId"] ?? "deepseek-r1:7b";

        try
        {
            // ساخت کلاینت OllamaSharp و تنظیم مدل
            var client = new OllamaApiClient(endpoint) { SelectedModel = modelId };

            var sb = new StringBuilder();
            await foreach (var part in client.GenerateAsync(request.Prompt).WithCancellation(ct))
            {
                sb.Append(part?.Response);
            }

            return Ok(new { model = modelId, response = sb.ToString() });
        }
        catch (Exception ex)
        {
            return StatusCode(502, new { error = "خطا در ارتباط با Ollama: " + ex.Message });
        }
    }
}
