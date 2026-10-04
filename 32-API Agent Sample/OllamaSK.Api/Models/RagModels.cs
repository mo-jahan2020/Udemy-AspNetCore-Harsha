namespace OllamaSK.Api.Models;

/// <summary>نتیجه جستجوی معنایی (RAG) — یک قطعه از سند با امتیاز شباهت</summary>
public sealed record ChunkHit(
    Guid DocId,
    string DocName,
    int Seq,
    float Score,
    string Content);

/// <summary>وضعیت زیرساخت RAG برای نمایش در صفحه و تست</summary>
public sealed record RagStatus(
    string EmbeddingModel,
    bool ModelAvailable,
    int ChunkCount,
    IReadOnlyList<IndexedDocInfo> IndexedDocuments);

/// <summary>سندی که قطعاتش در Vector Store ایندکس شده است</summary>
public sealed record IndexedDocInfo(Guid DocId, string DocName, int Chunks);
