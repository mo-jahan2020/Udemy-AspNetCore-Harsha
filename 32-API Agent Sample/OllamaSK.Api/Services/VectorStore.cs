using System.Runtime.InteropServices;
using Microsoft.Data.Sqlite;
using OllamaSK.Api.Models;

namespace OllamaSK.Api.Services;

/// <summary>ذخیره‌گاه وکتور (Vector Store) برای جستجوی معنایی RAG</summary>
public interface IVectorStore
{
    /// <summary>ذخیره قطعات سند همراه وکتورشان (ایندکس قبلی همان سند حذف می‌شود)</summary>
    void AddChunks(Guid docId, string docName, IReadOnlyList<(int Seq, string Content, float[] Vector)> chunks);

    /// <summary>جستجوی شباهت کسینوسی — docIds خالی یعنی در همه اسناد بگرد</summary>
    IReadOnlyList<ChunkHit> Search(float[] queryVector, Guid[]? docIds, int topK);

    /// <summary>تعداد کل قطعات ایندکس‌شده</summary>
    int Count();

    /// <summary>لیست اسناد ایندکس‌شده با تعداد قطعات</summary>
    IReadOnlyList<IndexedDocInfo> GetIndexedDocuments();

    /// <summary>حذف ایندکس یک سند</summary>
    bool DeleteByDocument(Guid docId);
}

/// <summary>
/// پیاده‌سازی Vector Store روی SQLite:
///   - وکتورها به‌صورت BLOB (بایت‌های float[]) ذخیره می‌شوند
///   - شباهت کسینوسی در C# محاسبه می‌شود
/// برای نمونه آموزشی با چند صد قطعه بسیار سریع است؛ اگر بعداً داده‌های میلیونی
/// داشتید می‌توانید همین اینترفیس را با Qdrant، pgvector یا Milvus جایگزین کنید.
/// </summary>
public sealed class SqliteVectorStore : IVectorStore
{
    private readonly SqliteConnectionFactory _db;

    public SqliteVectorStore(SqliteConnectionFactory db)
    {
        _db = db;

        using var connection = _db.Create();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS chunks (
                id         INTEGER PRIMARY KEY AUTOINCREMENT,
                doc_id     TEXT NOT NULL,
                doc_name   TEXT NOT NULL,
                seq        INTEGER NOT NULL,
                content    TEXT NOT NULL,
                dim        INTEGER NOT NULL,
                vector     BLOB NOT NULL,
                created_at TEXT NOT NULL
            );
            CREATE INDEX IF NOT EXISTS ix_chunks_doc ON chunks(doc_id);
            """;
        cmd.ExecuteNonQuery();
    }

    public void AddChunks(Guid docId, string docName, IReadOnlyList<(int Seq, string Content, float[] Vector)> chunks)
    {
        using var connection = _db.Create();
        using var transaction = connection.BeginTransaction();

        using (var delete = connection.CreateCommand())
        {
            delete.Transaction = transaction;
            delete.CommandText = "DELETE FROM chunks WHERE doc_id = $id;";
            delete.Parameters.AddWithValue("$id", docId.ToString());
            delete.ExecuteNonQuery();
        }

        foreach (var (seq, content, vector) in chunks)
        {
            using var cmd = connection.CreateCommand();
            cmd.Transaction = transaction;
            cmd.CommandText = """
                INSERT INTO chunks (doc_id, doc_name, seq, content, dim, vector, created_at)
                VALUES ($docId, $docName, $seq, $content, $dim, $vector, $now);
                """;
            cmd.Parameters.AddWithValue("$docId", docId.ToString());
            cmd.Parameters.AddWithValue("$docName", docName);
            cmd.Parameters.AddWithValue("$seq", seq);
            cmd.Parameters.AddWithValue("$content", content);
            cmd.Parameters.AddWithValue("$dim", vector.Length);
            cmd.Parameters.AddWithValue("$vector", ToBlob(vector));
            cmd.Parameters.AddWithValue("$now", DateTimeOffset.UtcNow.ToString("o"));
            cmd.ExecuteNonQuery();
        }

        transaction.Commit();
    }

    public IReadOnlyList<ChunkHit> Search(float[] queryVector, Guid[]? docIds, int topK)
    {
        using var connection = _db.Create();
        using var cmd = connection.CreateCommand();

        var sql = "SELECT doc_id, doc_name, seq, content, vector FROM chunks";
        if (docIds is { Length: > 0 })
        {
            // پارامترهای IN به‌صورت داینامیک ساخته می‌شوند (شناسه‌ها GUID تولیدی خودمان هستند)
            var names = new List<string>();
            for (var i = 0; i < docIds.Length; i++)
            {
                names.Add($"$doc{i}");
                cmd.Parameters.AddWithValue($"$doc{i}", docIds[i].ToString());
            }
            sql += $" WHERE doc_id IN ({string.Join(", ", names)})";
        }
        cmd.CommandText = sql;

        var scored = new List<ChunkHit>();
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                var vector = FromBlob((byte[])reader["vector"]);
                var score = CosineSimilarity(queryVector, vector);
                scored.Add(new ChunkHit(
                    Guid.Parse(reader.GetString(0)),
                    reader.GetString(1),
                    reader.GetInt32(2),
                    score,
                    reader.GetString(3)));
            }
        }

        // امتیاز یکسان نیست — نزولی مرتب و topK برمی‌داریم
        return scored
            .OrderByDescending(h => h.Score)
            .Take(topK)
            .ToList();
    }

    public int Count()
    {
        using var connection = _db.Create();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM chunks;";
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public IReadOnlyList<IndexedDocInfo> GetIndexedDocuments()
    {
        using var connection = _db.Create();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT doc_id, doc_name, COUNT(*) FROM chunks GROUP BY doc_id, doc_name ORDER BY MAX(rowid) DESC;";

        var list = new List<IndexedDocInfo>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new IndexedDocInfo(
                Guid.Parse(reader.GetString(0)),
                reader.GetString(1),
                reader.GetInt32(2)));
        }
        return list;
    }

    public bool DeleteByDocument(Guid docId)
    {
        using var connection = _db.Create();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM chunks WHERE doc_id = $id;";
        cmd.Parameters.AddWithValue("$id", docId.ToString());
        return cmd.ExecuteNonQuery() > 0;
    }

    // ─────────────────────────────── کمکی ───────────────────────────────

    /// <summary>شباهت کسینوسی دو وکتور (بازه −۱ تا +۱)</summary>
    private static float CosineSimilarity(float[] a, float[] b)
    {
        if (a.Length == 0 || a.Length != b.Length)
            return 0f;

        float dot = 0, normA = 0, normB = 0;
        for (var i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            normA += a[i] * a[i];
            normB += b[i] * b[i];
        }

        if (normA == 0 || normB == 0)
            return 0f;

        return dot / (MathF.Sqrt(normA) * MathF.Sqrt(normB));
    }

    /// <summary>float[] → BLOB (بایت‌های little-endian)</summary>
    private static byte[] ToBlob(float[] vector)
    {
        var bytes = new byte[vector.Length * sizeof(float)];
        MemoryMarshal.AsBytes(vector.AsSpan()).CopyTo(bytes);
        return bytes;
    }

    /// <summary>BLOB → float[]</summary>
    private static float[] FromBlob(byte[] blob)
    {
        var floats = new float[blob.Length / sizeof(float)];
        MemoryMarshal.Cast<byte, float>(blob.AsSpan()).CopyTo(floats);
        return floats;
    }
}
