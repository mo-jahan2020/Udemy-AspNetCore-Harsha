using System.Text;
using Microsoft.Extensions.Options;
using OllamaSK.Api.Models;

namespace OllamaSK.Api.Services;

/// <summary>تنظیمات RAG — از بخش «Rag» در appsettings.json خوانده می‌شود</summary>
public sealed class RagOptions
{
    /// <summary>اندازه هر قطعه (کاراکتر)</summary>
    public int ChunkSize { get; set; } = 900;

    /// <summary>هم‌پوشانی قطعات متوالی تا بریدن جمله‌ها آسیب نزند</summary>
    public int ChunkOverlap { get; set; } = 150;

    /// <summary>تعداد قطعات بازیابی‌شده برای هر سؤال</summary>
    public int TopK { get; set; } = 4;

    /// <summary>سقف کاراکتر متن زمینه ساخته‌شده برای مدل</summary>
    public int MaxContextChars { get; set; } = 4500;
}

/// <summary>سرویس RAG: ایندکس معنایی اسناد و بازیابی قطعات مرتبط برای پرامپت</summary>
public interface IRagService
{
    /// <summary>قطعه‌بندی + وکتورسازی + ذخیره یک سند (هرگز Exception نمی‌دهد؛ نتیجه را برمی‌گرداند)</summary>
    Task<(bool Indexed, int Chunks, string? Note)> IndexDocumentAsync(UploadedDocument doc, CancellationToken ct = default);

    /// <summary>ساخت متن زمینه از قطعات مرتبط با سؤال (اگر RAG فعال نبود، Used=false)</summary>
    Task<(bool Used, string Context)> BuildContextAsync(string question, Guid[]? docIds, CancellationToken ct = default);

    /// <summary>ایندکس مجدد همه اسناد موجود (مثلاً بعد از نصب مدل Embedding)</summary>
    Task<(int Indexed, int TotalChunks, List<string> Errors)> ReindexAllAsync(CancellationToken ct = default);

    /// <summary>وضعیت زیرساخت RAG</summary>
    Task<RagStatus> GetStatusAsync();
}

/// <summary>
/// چرخه RAG در این پروژه:
///   ۱) آپلود سند  →  قطعه‌بندی (Chunking)  →  Embedding با nomic-embed-text  →  ذخیره وکتور در SQLite
///   ۲) هر سؤال چت  →  Embedding سؤال  →  جستجوی شباهت کسینوسی  →  top-K قطعه مرتبط  →  تزریق در پرامپت
/// اگر مدل Embedding نصب نباشد، به‌صورت خودکار به حالت «تزریق متن کامل سند» برمی‌گردیم (Fallback).
/// </summary>
public sealed class RagService(
    IEmbeddingService embedding,
    IVectorStore vectorStore,
    IDocumentStore documents,
    IOptions<RagOptions> options,
    ILogger<RagService> logger) : IRagService
{
    private readonly RagOptions _opt = options.Value;

    public async Task<(bool Indexed, int Chunks, string? Note)> IndexDocumentAsync(UploadedDocument doc, CancellationToken ct = default)
    {
        var chunks = ChunkText(doc.Content, _opt.ChunkSize, _opt.ChunkOverlap);
        if (chunks.Count == 0)
            return (false, 0, "متنی برای ایندکس کردن پیدا نشد.");

        if (!await embedding.IsModelAvailableAsync(ct))
            return (false, 0, $"مدل Embedding «{embedding.ModelId}» در دسترس نیست (نصب: ollama pull {embedding.ModelId})");

        try
        {
            var embeddings = await embedding.EmbedAsync(chunks, ct);

            var payload = new List<(int Seq, string Content, float[] Vector)>();
            for (var i = 0; i < chunks.Count; i++)
                payload.Add((i, chunks[i], embeddings[i]));

            vectorStore.AddChunks(doc.Id, doc.FileName, payload);

            logger.LogInformation("سند «{File}» ایندکس شد: {Count} قطعه با مدل {Model}", doc.FileName, chunks.Count, embedding.ModelId);
            return (true, chunks.Count, null);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "ایندکس سند «{File}» ناموفق بود", doc.FileName);
            return (false, 0, "خطا در تولید وکتور: " + ex.Message);
        }
    }

    public async Task<(bool Used, string Context)> BuildContextAsync(string question, Guid[]? docIds, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(question) || docIds is not { Length: > 0 })
            return (false, string.Empty);

        if (!await embedding.IsModelAvailableAsync(ct))
            return (false, string.Empty);

        try
        {
            var questionVector = (await embedding.EmbedAsync([question], ct)).FirstOrDefault();
            if (questionVector is null)
                return (false, string.Empty);

            var hits = vectorStore.Search(questionVector, docIds, _opt.TopK);
            if (hits.Count == 0)
                return (false, string.Empty);

            var sb = new StringBuilder("### قطعات مرتبط از اسناد پیوست (بازیابی معنایی):\n");
            var budget = _opt.MaxContextChars;

            for (var i = 0; i < hits.Count; i++)
            {
                var hit = hits[i];
                var content = hit.Content;

                if (content.Length > budget)
                    content = content[..budget];
                budget -= content.Length;

                sb.AppendLine($"\n[{i + 1}] سند «{hit.DocName}» — بخش {hit.Seq + 1} (امتیاز شباهت {hit.Score:0.###}):");
                sb.AppendLine(content);

                if (budget <= 0)
                    break;
            }

            return (true, sb.ToString());
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "ساخت زمینه RAG ناموفق بود؛ به حالت بدون RAG برمی‌گردیم");
            return (false, string.Empty);
        }
    }

    public async Task<(int Indexed, int TotalChunks, List<string> Errors)> ReindexAllAsync(CancellationToken ct = default)
    {
        var indexed = 0;
        var totalChunks = 0;
        var errors = new List<string>();

        foreach (var doc in documents.GetAll())
        {
            var (ok, chunks, note) = await IndexDocumentAsync(doc, ct);
            if (ok)
            {
                indexed++;
                totalChunks += chunks;
            }
            else
            {
                errors.Add($"«{doc.FileName}»: {note}");
            }
        }

        return (indexed, totalChunks, errors);
    }

    public async Task<RagStatus> GetStatusAsync()
        => new(
            embedding.ModelId,
            await embedding.IsModelAvailableAsync(),
            vectorStore.Count(),
            vectorStore.GetIndexedDocuments());

    // ─────────────────────────────── قطعه‌بندی متن ───────────────────────────────

    /// <summary>
    /// تقسیم متن به قطعات هم‌اندازه با هم‌پوشانی:
    /// اول پاراگراف‌ها تا اندازه مجاز بسته‌بندی می‌شوند؛ پاراگراف‌های خیلی بلند
    /// با پنجره لغزان بریده می‌شوند تا جمله‌ها تا حد ممکن سالم بمانند.
    /// </summary>
    public static List<string> ChunkText(string text, int size, int overlap)
    {
        var chunks = new List<string>();
        if (string.IsNullOrWhiteSpace(text))
            return chunks;

        if (size < 100) size = 100;
        if (overlap < 0) overlap = 0;
        if (overlap >= size) overlap = size / 4;

        var paragraphs = text.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var current = new StringBuilder();

        void Flush()
        {
            var value = current.ToString().Trim();
            if (value.Length > 0)
                chunks.Add(value);
            current.Clear();
        }

        foreach (var paragraph in paragraphs)
        {
            if (paragraph.Length > size)
            {
                Flush();

                // پنجره لغزان روی پاراگراف بلند
                var step = Math.Max(1, size - overlap);
                for (var start = 0; start < paragraph.Length; start += step)
                {
                    var len = Math.Min(size, paragraph.Length - start);
                    chunks.Add(paragraph.Substring(start, len));
                    if (start + len >= paragraph.Length)
                        break;
                }
                continue;
            }

            if (current.Length + paragraph.Length + 1 > size)
                Flush();

            if (current.Length > 0)
                current.Append('\n');
            current.Append(paragraph);
        }

        Flush();
        return chunks;
    }
}
