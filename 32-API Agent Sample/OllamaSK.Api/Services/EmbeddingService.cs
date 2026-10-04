using System.Text.Json;
using OllamaSharp;
using OllamaSharp.Models;

namespace OllamaSK.Api.Services;

/// <summary>سرویس تولید Embedding (وکتور معنایی) با مدل محلی Ollama</summary>
public interface IEmbeddingService
{
    /// <summary>نام مدل Embedding (مثلاً nomic-embed-text)</summary>
    string ModelId { get; }

    /// <summary>آیا مدل Embedding در Ollama نصب و در دسترس است؟ (نتیجه ۶۰ ثانیه Cache می‌شود)</summary>
    Task<bool> IsModelAvailableAsync(CancellationToken ct = default);

    /// <summary>وکتور گروهی از متن‌ها را یکجا تولید می‌کند (تعداد وکتور = تعداد متن‌ها)</summary>
    Task<List<float[]>> EmbedAsync(IReadOnlyList<string> texts, CancellationToken ct = default);
}

/// <summary>
/// تولید Embedding از طریق پکیج OllamaSharp (متد EmbedAsync — همان endpoint «/api/embed»).
/// مدل پیشنهادی: nomic-embed-text (کوچک، سریع و رایگان). برای نصب: ollama pull nomic-embed-text
/// </summary>
public sealed class EmbeddingService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<EmbeddingService> logger) : IEmbeddingService
{
    private bool? _cachedAvailability;
    private DateTimeOffset _checkedAt = DateTimeOffset.MinValue;

    public string ModelId => configuration["Ollama:EmbeddingModelId"] ?? "nomic-embed-text";

    public async Task<bool> IsModelAvailableAsync(CancellationToken ct = default)
    {
        // Cache کوتاه تا در هر پیام چت، درخواست تکراری به Ollama زده نشود
        if (_cachedAvailability.HasValue && DateTimeOffset.UtcNow - _checkedAt < TimeSpan.FromSeconds(60))
            return _cachedAvailability.Value;

        try
        {
            var client = httpClientFactory.CreateClient("ollama");
            var json = await client.GetStringAsync("/api/tags", ct);
            using var doc = JsonDocument.Parse(json);

            var found = false;
            foreach (var model in doc.RootElement.GetProperty("models").EnumerateArray())
            {
                var name = model.TryGetProperty("model", out var mv) ? mv.GetString()
                         : model.TryGetProperty("name", out var nv) ? nv.GetString()
                         : null;

                if (name is null)
                    continue;

                // مقایسه نام کامل و نام پایه (nomic-embed-text با nomic-embed-text:latest یکی است)
                if (string.Equals(name, ModelId, StringComparison.OrdinalIgnoreCase)
                    || name.Split(':')[0].Equals(ModelId, StringComparison.OrdinalIgnoreCase))
                {
                    found = true;
                    break;
                }
            }

            _cachedAvailability = found;
            if (!found)
                logger.LogWarning("مدل Embedding «{Model}» در Ollama پیدا نشد؛ RAG غیرفعال است. نصب: ollama pull {Model}", ModelId, ModelId);
        }
        catch (Exception ex)
        {
            logger.LogWarning("بررسی دسترسی Ollama برای Embedding ناموفق بود: {Message}", ex.Message);
            _cachedAvailability = false;
        }

        _checkedAt = DateTimeOffset.UtcNow;
        return _cachedAvailability.Value;
    }

    public async Task<List<float[]>> EmbedAsync(IReadOnlyList<string> texts, CancellationToken ct = default)
    {
        if (texts.Count == 0)
            return [];

        var endpoint = configuration["Ollama:Endpoint"] ?? "http://localhost:11434";
        var client = new OllamaApiClient(endpoint);

        // EmbedAsync یک درخواست با ورودی چندتایی می‌فرستد (بچ) — سریع‌تر از فراخوانی تکی
        var response = await client.EmbedAsync(new EmbedRequest
        {
            Model = ModelId,
            Input = [.. texts]
        }, ct);

        var vectors = response.Embeddings ?? [];
        if (vectors.Count != texts.Count)
            throw new InvalidOperationException($"تعداد وکتورهای دریافتی ({vectors.Count}) با تعداد متن‌ها ({texts.Count}) یکی نیست.");

        return vectors;
    }
}
