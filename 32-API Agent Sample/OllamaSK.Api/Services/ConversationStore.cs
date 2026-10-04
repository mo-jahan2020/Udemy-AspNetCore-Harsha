using System.Globalization;
using Microsoft.Data.Sqlite;
using OllamaSK.Api.Models;

namespace OllamaSK.Api.Services;

/// <summary>ذخیره‌گاه پایا (SQLite) تاریخچه مکالمات</summary>
public interface IConversationStore
{
    /// <summary>یافتن مکالمه موجود یا ساخت مکالمه جدید با عنوان خودکار</summary>
    ConversationInfo EnsureConversation(Guid? id, string seedTitle);

    /// <summary>لیست همه مکالمات (آخرین فعالیت اول)</summary>
    IReadOnlyList<ConversationInfo> GetConversations();

    /// <summary>پیام‌های یک مکالمه با سقف تعداد و سقف کاراکتر (برای ساخت ChatHistory)</summary>
    IReadOnlyList<ConversationMessage> GetMessages(Guid conversationId, int maxCount, int maxChars);

    /// <summary>ذخیره پیام جدید و به‌روزرسانی زمان فعالیت مکالمه</summary>
    void AddMessage(Guid conversationId, string role, string content, string? thinking);

    /// <summary>تغییر عنوان مکالمه</summary>
    bool Rename(Guid id, string title);

    /// <summary>حذف مکالمه و همه پیام‌هایش</summary>
    bool Delete(Guid id);

    /// <summary>حذف همه مکالمات</summary>
    void DeleteAll();
}

/// <summary>
/// پیاده‌سازی SQLite از تاریخچه مکالمات — بعد از ری‌استارت برنامه هم پیام‌ها می‌مانند.
/// جداول با «IF NOT EXISTS» ساخته می‌شوند تا اولین اجرا هم بی‌دردسر باشد.
/// </summary>
public sealed class ConversationStore : IConversationStore
{
    private readonly SqliteConnectionFactory _db;

    public ConversationStore(SqliteConnectionFactory db)
    {
        _db = db;

        // ساخت جداول در اولین اجرا
        using var connection = _db.Create();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS conversations (
                id         TEXT PRIMARY KEY,
                title      TEXT NOT NULL,
                created_at TEXT NOT NULL,
                updated_at TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS messages (
                id              INTEGER PRIMARY KEY AUTOINCREMENT,
                conversation_id TEXT NOT NULL,
                role            TEXT NOT NULL,
                content         TEXT NOT NULL,
                thinking        TEXT,
                created_at      TEXT NOT NULL
            );
            CREATE INDEX IF NOT EXISTS ix_messages_conv ON messages(conversation_id);
            """;
        cmd.ExecuteNonQuery();
    }

    public ConversationInfo EnsureConversation(Guid? id, string seedTitle)
    {
        // اگر شناسه داده شده و مکالمه موجود است، همان را برگردان
        if (id is { } existing && TryGet(existing, out var info))
            return info;

        // در غیر این صورت مکالمه جدید (اگر شناسه نامعتبر بود، شناسه تازه می‌سازیم)
        var newId = Guid.NewGuid();
        var title = Truncate(string.IsNullOrWhiteSpace(seedTitle) ? "مکالمه جدید" : seedTitle.Trim(), 60);
        var now = DateTimeOffset.UtcNow.ToString("o", CultureInfo.InvariantCulture);

        using var connection = _db.Create();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "INSERT INTO conversations (id, title, created_at, updated_at) VALUES ($id, $title, $now, $now);";
        cmd.Parameters.AddWithValue("$id", newId.ToString());
        cmd.Parameters.AddWithValue("$title", title);
        cmd.Parameters.AddWithValue("$now", now);
        cmd.ExecuteNonQuery();

        return new ConversationInfo(newId, title, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, 0);
    }

    public IReadOnlyList<ConversationInfo> GetConversations()
    {
        using var connection = _db.Create();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT c.id, c.title, c.created_at, c.updated_at,
                   (SELECT COUNT(*) FROM messages m WHERE m.conversation_id = c.id) AS msg_count
            FROM conversations c
            ORDER BY c.updated_at DESC;
            """;

        var list = new List<ConversationInfo>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new ConversationInfo(
                Guid.Parse(reader.GetString(0)),
                reader.GetString(1),
                ParseDate(reader.GetString(2)),
                ParseDate(reader.GetString(3)),
                (int)reader.GetInt64(4)));
        }
        return list;
    }

    public IReadOnlyList<ConversationMessage> GetMessages(Guid conversationId, int maxCount, int maxChars)
    {
        // آخرین پیام‌ها را از آخر به اول می‌خوانیم و در سقف کاراکتر می‌ایستیم
        using var connection = _db.Create();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT id, role, content, thinking, created_at
            FROM messages
            WHERE conversation_id = $id
            ORDER BY id DESC
            LIMIT $limit;
            """;
        cmd.Parameters.AddWithValue("$id", conversationId.ToString());
        cmd.Parameters.AddWithValue("$limit", maxCount);

        var taken = new List<ConversationMessage>();
        var budget = maxChars;

        using var reader = cmd.ExecuteReader();
        while (reader.Read() && budget > 0)
        {
            var content = reader.GetString(2);
            if (content.Length > budget)
                content = content[..budget]; // برش از آخرِ پیام قدیمی؛ پیام فعلی همیشه کامل می‌ماند

            budget -= content.Length;
            taken.Add(new ConversationMessage(
                reader.GetInt64(0),
                reader.GetString(1),
                content,
                reader.IsDBNull(3) ? null : reader.GetString(3),
                ParseDate(reader.GetString(4))));
        }

        taken.Reverse(); // ترتیب زمانی طبیعی برای ChatHistory
        return taken;
    }

    public void AddMessage(Guid conversationId, string role, string content, string? thinking)
    {
        var now = DateTimeOffset.UtcNow.ToString("o", CultureInfo.InvariantCulture);

        using var connection = _db.Create();
        using var transaction = connection.BeginTransaction();

        using (var cmd = connection.CreateCommand())
        {
            cmd.Transaction = transaction;
            cmd.CommandText = """
                INSERT INTO messages (conversation_id, role, content, thinking, created_at)
                VALUES ($id, $role, $content, $thinking, $now);
                """;
            cmd.Parameters.AddWithValue("$id", conversationId.ToString());
            cmd.Parameters.AddWithValue("$role", role);
            cmd.Parameters.AddWithValue("$content", content);
            cmd.Parameters.AddWithValue("$thinking", (object?)thinking ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$now", now);
            cmd.ExecuteNonQuery();
        }

        using (var cmd = connection.CreateCommand())
        {
            cmd.Transaction = transaction;
            cmd.CommandText = "UPDATE conversations SET updated_at = $now WHERE id = $id;";
            cmd.Parameters.AddWithValue("$now", now);
            cmd.Parameters.AddWithValue("$id", conversationId.ToString());
            cmd.ExecuteNonQuery();
        }

        transaction.Commit();
    }

    public bool Rename(Guid id, string title)
    {
        using var connection = _db.Create();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "UPDATE conversations SET title = $title WHERE id = $id;";
        cmd.Parameters.AddWithValue("$title", Truncate(string.IsNullOrWhiteSpace(title) ? "بدون عنوان" : title.Trim(), 60));
        cmd.Parameters.AddWithValue("$id", id.ToString());
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool Delete(Guid id)
    {
        using var connection = _db.Create();
        using var transaction = connection.BeginTransaction();

        long removed;
        using (var cmd = connection.CreateCommand())
        {
            cmd.Transaction = transaction;
            cmd.CommandText = "DELETE FROM messages WHERE conversation_id = $id; DELETE FROM conversations WHERE id = $id;";
            cmd.Parameters.AddWithValue("$id", id.ToString());
            removed = cmd.ExecuteNonQuery();
        }

        transaction.Commit();
        return removed > 0;
    }

    public void DeleteAll()
    {
        using var connection = _db.Create();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM messages; DELETE FROM conversations;";
        cmd.ExecuteNonQuery();
    }

    // ─────────────────────────────── کمکی ───────────────────────────────

    private bool TryGet(Guid id, out ConversationInfo info)
    {
        using var connection = _db.Create();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT c.title, c.created_at, c.updated_at,
                   (SELECT COUNT(*) FROM messages m WHERE m.conversation_id = c.id)
            FROM conversations c WHERE c.id = $id;
            """;
        cmd.Parameters.AddWithValue("$id", id.ToString());

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            info = new ConversationInfo(
                id,
                reader.GetString(0),
                ParseDate(reader.GetString(1)),
                ParseDate(reader.GetString(2)),
                (int)reader.GetInt64(3));
            return true;
        }

        info = null!;
        return false;
    }

    private static DateTimeOffset ParseDate(string value)
        => DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var result)
            ? result
            : DateTimeOffset.UtcNow;

    private static string Truncate(string text, int max)
        => text.Length <= max ? text : text[..max] + "…";
}
