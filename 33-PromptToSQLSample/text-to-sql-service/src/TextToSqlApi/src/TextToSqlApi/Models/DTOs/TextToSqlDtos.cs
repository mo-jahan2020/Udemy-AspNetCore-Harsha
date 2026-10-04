using System.ComponentModel.DataAnnotations;

namespace TextToSqlApi.Models.DTOs;

/// <summary>درخواست تولید SQL از روی پرامپت زبانی.</summary>
public class GenerateSqlRequest
{
    /// <summary>شناسه نشست کاربر.</summary>
    [Required]
    public Guid SessionId { get; set; }

    /// <summary>پرامپت زبانی کاربر به زبان طبیعی.</summary>
    [Required, MinLength(1)]
    public string Prompt { get; set; } = string.Empty;
}

/// <summary>پاسخ تولید SQL.</summary>
public class GenerateSqlResponse
{
    /// <summary>SQL تولیدشده توسط مدل.</summary>
    public string Sql { get; set; } = string.Empty;

    /// <summary>شناسه نشست.</summary>
    public Guid SessionId { get; set; }

    /// <summary>زمان پاسخ به میلی‌ثانیه.</summary>
    public long ResponseTimeMs { get; set; }

    /// <summary>نام مدلی که SQL را تولید کرده.</summary>
    public string ModelName { get; set; } = string.Empty;

    /// <summary>منبع تولید (Vanna یا Ollama-Fallback).</summary>
    public string Source { get; set; } = string.Empty;
}

/// <summary>درخواست ثبت/به‌روزرسانی شما دیتابیس هدف.</summary>
public class UpdateSchemaRequest
{
    /// <summary>شمای دیتابیس به‌صورت متنی (DDL جداول + کلیدهای خارجی + نمونه داده اختیاری).</summary>
    [Required, MinLength(1)]
    public string SchemaText { get; set; } = string.Empty;
}

/// <summary>درخواست اجرای SQL (فقط Read-Only).</summary>
public class ExecuteSqlRequest
{
    /// <summary>SQL تولیدشده برای اجرا.</summary>
    [Required, MinLength(1)]
    public string Sql { get; set; } = string.Empty;

    /// <summary>حداکثر تعداد ردیف‌های بازگشتی.</summary>
    [Range(1, 1000)]
    public int MaxRows { get; set; } = 100;
}

/// <summary>پاسخ اجرای SQL شامل ستون‌ها و ردیف‌ها.</summary>
public class ExecuteSqlResponse
{
    public List<string> Columns { get; set; } = new();
    public List<Dictionary<string, object?>> Rows { get; set; } = new();
    public int RowCount { get; set; }
    public long ExecutionTimeMs { get; set; }
}
