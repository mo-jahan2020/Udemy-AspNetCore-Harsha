namespace OllamaSK.Api.Models;

/// <summary>درخواست چت (پیام کاربر + اختیاری شناسه مکالمه + اختیاری شناسه اسناد پیوست‌شده)</summary>
public sealed record ChatRequest(string Message, Guid? ConversationId = null, Guid[]? DocumentIds = null);

/// <summary>
/// پاسخ چت شامل جواب، بخش «تفکر» مدل، اطلاعات ابزارها و مکالمه.
/// RagUsed=true یعنی متن زمینه از جستجوی معنایی (وکتوری) اسناد ساخته شده است.
/// </summary>
public sealed record ChatResponse(
    string Answer,
    string? Thinking,
    int DocsUsed,
    bool ToolsEnabled,
    bool RagUsed,
    Guid ConversationId,
    string ConversationTitle);

/// <summary>یک قطعه از پاسخ استریم (Server-Sent Events)</summary>
/// <remarks>
/// مقادیر Type عبارت‌اند از:
/// meta → اطلاعات اولیه (JSON) | thinking → متن تفکر مدل | answer → پاسخ نهایی
/// error → پیام خطا | done → پایان پاسخ
/// </remarks>
public sealed record StreamChunk(string Type, string Content);

/// <summary>محتوای JSON رویداد «meta» در استریم (به‌صورت camelCase سریالایز می‌شود)</summary>
public sealed record StreamMeta(int DocsUsed, bool Tools, bool Rag, Guid ConversationId, string ConversationTitle);

/// <summary>درخواست فراخوانی مستقیم Plugin ماشین‌حساب</summary>
public sealed record CalcRequest(string Operation, double A, double B);
