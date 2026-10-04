using System.Text.Json;

namespace OllamaSK.Api.Services;

/// <summary>
/// بررسی قابلیت‌های مدل نصب‌شده در Ollama (مخصوصاً پشتیبانی از Function Calling).
/// این سرویس یک‌بار از endpoint «/api/tags» استعلام می‌گیرد و نتیجه را Cache می‌کند.
/// </summary>
public sealed class OllamaCapabilities(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<OllamaCapabilities> logger)
{
    private bool? _supportsTools;

    /// <summary>
    /// آیا مدل فعلی از Function Calling (ابزارها) پشتیبانی می‌کند؟
    /// مدل‌های خلاصه‌شده (Distill) مثل deepseek-r1 معمولاً این قابلیت را ندارند
    /// و مدل‌هایی مثل qwen2.5 یا llama3.1 دارند.
    /// </summary>
    public async Task<bool> SupportsToolCallingAsync(CancellationToken ct = default)
    {
        if (_supportsTools.HasValue)
            return _supportsTools.Value;

        try
        {
            var client = httpClientFactory.CreateClient("ollama");
            var json = await client.GetStringAsync("/api/tags", ct);
            using var doc = JsonDocument.Parse(json);

            var modelId = configuration["Ollama:ModelId"] ?? string.Empty;
            var baseName = modelId.Split(':')[0]; // مثلاً deepseek-r1 از deepseek-r1:7b

            foreach (var model in doc.RootElement.GetProperty("models").EnumerateArray())
            {
                // در نسخه‌های جدید فیلد «model» و در نسخه‌های قدیمی‌تر «name» است
                var name = model.TryGetProperty("model", out var mv) ? mv.GetString()
                         : model.TryGetProperty("name", out var nv) ? nv.GetString()
                         : null;

                if (string.IsNullOrEmpty(name))
                    continue;

                var nameBase = name.Split(':')[0];
                var isTarget = string.Equals(name, modelId, StringComparison.OrdinalIgnoreCase)
                            || string.Equals(nameBase, baseName, StringComparison.OrdinalIgnoreCase);

                if (!isTarget)
                    continue;

                if (model.TryGetProperty("capabilities", out var caps))
                {
                    var hasTools = caps.EnumerateArray().Any(c => c.GetString() == "tools");
                    _supportsTools = hasTools;
                    logger.LogInformation("پشتیبانی مدل {Model} از Function Calling: {HasTools}", name, hasTools);
                    return _supportsTools.Value;
                }
            }

            // اطلاعات قابلیت‌ها موجود نبود (نسخه قدیمی Ollama)؛ به سرور اعتماد می‌کنیم
            _supportsTools = true;
        }
        catch (Exception ex)
        {
            logger.LogWarning("بررسی قابلیت‌های Ollama ناموفق بود: {Message}", ex.Message);
            _supportsTools = true;
        }

        return _supportsTools.Value;
    }
}
