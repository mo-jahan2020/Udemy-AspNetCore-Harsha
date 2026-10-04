using Microsoft.Data.Sqlite;
using OllamaSK.Api.Models;

namespace OllamaSK.Api.Services;

public interface IDocumentStore
{
    /// <summary>افزودن سند جدید و برگرداندن آن</summary>
    UploadedDocument Add(UploadedDocument document);

    /// <summary>لیست همه اسناد (جدیدترین اول)</summary>
    IReadOnlyList<UploadedDocument> GetAll();

    /// <summary>یافتن سند بر اساس شناسه</summary>
    UploadedDocument? Get(Guid id);

    /// <summary>حذف سند</summary>
    bool Remove(Guid id);
}

/// <summary>
/// ذخیره‌گاه پایا (SQLite) اسناد آپلودشده — برخلاف نسخه اولیه درون‌حافظه‌ای،
/// بعد از ری‌استارت برنامه هم اسناد و متن استخراج‌شده باقی می‌مانند
/// و ایندکس RAG آن‌ها همچنان معتبر است.
/// </summary>
public sealed class DocumentStore : IDocumentStore
{
    private readonly SqliteConnectionFactory _db;

    public DocumentStore(SqliteConnectionFactory db)
    {
        _db = db;

        using var connection = db.Create();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS documents (
                id            TEXT PRIMARY KEY,
                file_name     TEXT NOT NULL,
                file_kind     TEXT NOT NULL,
                size_bytes    INTEGER NOT NULL,
                content       TEXT NOT NULL,
                content_chars INTEGER NOT NULL,
                uploaded_at   TEXT NOT NULL
            );
            """;
        cmd.ExecuteNonQuery();
    }

    public UploadedDocument Add(UploadedDocument document)
    {
        using var connection = _db.Create();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            INSERT OR REPLACE INTO documents (id, file_name, file_kind, size_bytes, content, content_chars, uploaded_at)
            VALUES ($id, $name, $kind, $size, $content, $chars, $at);
            """;
        cmd.Parameters.AddWithValue("$id", document.Id.ToString());
        cmd.Parameters.AddWithValue("$name", document.FileName);
        cmd.Parameters.AddWithValue("$kind", document.FileKind);
        cmd.Parameters.AddWithValue("$size", document.SizeBytes);
        cmd.Parameters.AddWithValue("$content", document.Content);
        cmd.Parameters.AddWithValue("$chars", document.ContentChars);
        cmd.Parameters.AddWithValue("$at", document.UploadedAt.ToString("o"));
        cmd.ExecuteNonQuery();

        return document;
    }

    public IReadOnlyList<UploadedDocument> GetAll()
    {
        using var connection = _db.Create();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT id, file_name, file_kind, size_bytes, content, content_chars, uploaded_at FROM documents ORDER BY uploaded_at DESC;";
        return ReadAll(cmd);
    }

    public UploadedDocument? Get(Guid id)
    {
        using var connection = _db.Create();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT id, file_name, file_kind, size_bytes, content, content_chars, uploaded_at FROM documents WHERE id = $id;";
        cmd.Parameters.AddWithValue("$id", id.ToString());
        return ReadAll(cmd).FirstOrDefault();
    }

    public bool Remove(Guid id)
    {
        using var connection = _db.Create();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM documents WHERE id = $id;";
        cmd.Parameters.AddWithValue("$id", id.ToString());
        return cmd.ExecuteNonQuery() > 0;
    }

    private static List<UploadedDocument> ReadAll(SqliteCommand cmd)
    {
        var list = new List<UploadedDocument>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new UploadedDocument
            {
                Id = Guid.Parse(reader.GetString(0)),
                FileName = reader.GetString(1),
                FileKind = reader.GetString(2),
                SizeBytes = reader.GetInt64(3),
                Content = reader.GetString(4),
                ContentChars = reader.GetInt32(5),
                UploadedAt = DateTimeOffset.TryParse(reader.GetString(6), out var at) ? at : DateTimeOffset.UtcNow
            });
        }
        return list;
    }
}
