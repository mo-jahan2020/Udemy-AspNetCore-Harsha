using Microsoft.Data.Sqlite;

namespace OllamaSK.Api.Services;

/// <summary>
/// کارخانه اتصال SQLite — مسیر فایل دیتابیس از بخش «Database» در appsettings خوانده می‌شود.
/// سه ذخیره‌گاه از این کلاس استفاده می‌کنند: مکالمات، اسناد و وکتورهای RAG.
/// SQLite برای نمونه آموزشی انتخاب شده چون به هیچ سرور دیتابیس نیاز ندارد؛
/// در پروژه واقعی می‌توانید همین اینترفیس‌ها را با PostgreSQL/pgvector یا SQL Server جایگزین کنید.
/// </summary>
public sealed class SqliteConnectionFactory(IConfiguration configuration, IHostEnvironment environment)
{
    private readonly string _filePath = ResolvePath(configuration, environment);

    public string ConnectionString { get; } = new SqliteConnectionStringBuilder
    {
        Mode = SqliteOpenMode.ReadWriteCreate,
        Cache = SqliteCacheMode.Shared,
        DataSource = ResolvePath(configuration, environment)
    }.ToString();

    /// <summary>ساخت اتصال باز جدید (هر تماس یک اتصال تازه)</summary>
    public SqliteConnection Create()
    {
        var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        // WAL برای خواندن/نوشتن هم‌زمان بهتر + روی دیسک ماندگار می‌ماند
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "PRAGMA journal_mode=WAL;";
        cmd.ExecuteNonQuery();

        return connection;
    }

    /// <summary>مسیر فایل دیتابیس نسبت به ریشه برنامه (پیش‌فرض App_Data/ollamask.db)</summary>
    private static string ResolvePath(IConfiguration configuration, IHostEnvironment environment)
    {
        var configured = configuration["Database:FileName"] ?? "App_Data/ollamask.db";
        var fullPath = Path.IsPathRooted(configured)
            ? configured
            : Path.Combine(environment.ContentRootPath, configured.Replace('/', Path.DirectorySeparatorChar));

        var dir = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        return fullPath;
    }

    /// <summary>نمایش مسیر فایل در API وضعیت</summary>
    public string FilePath => _filePath;
}
