using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using OllamaSK.Api.Models;

namespace OllamaSK.Api.Services;

public interface IChatService
{
    /// <summary>ارسال پیام و دریافت پاسخ کامل (بدون استریم)</summary>
    Task<ChatResponse> AskAsync(ChatRequest request, CancellationToken ct = default);

    /// <summary>ارسال پیام و دریافت پاسخ استریم با جداسازی بخش «تفکر» مدل</summary>
    IAsyncEnumerable<StreamChunk> AskStreamAsync(ChatRequest request, CancellationToken ct = default);
}

/// <summary>
/// سرویس اصلی چت: از طریق Semantic Kernel به مدل Ollama متصل می‌شود.
///   - تاریخچه گفتگو در SQLite ذخیره و در هر درخواست به ChatHistory اضافه می‌شود (حافظه پایا)
///   - اگر سندی پیوست شده باشد، اول RAG (جستجوی معنایی) امتحان می‌شود؛ در نبود مدل Embedding،
///     متن کامل سند (کوتاه‌شده) تزریق می‌شود
///   - Function Calling فقط وقتی فعال می‌شود که مدل واقعاً از ابزارها پشتیبانی کند
/// </summary>
public sealed class ChatService(
    Kernel kernel,
    IDocumentStore store,
    IConversationStore conversations,
    IRagService rag,
    OllamaCapabilities capabilities,
    IConfiguration configuration,
    ILogger<ChatService> logger) : IChatService
{
    private const string SystemPrompt =
        "تو یک دستیار هوشمند فارسی‌زبان هستی. " +
        "پاسخ‌ها را دقیق، کمک‌کننده و به زبان فارسی ارائه بده. " +
        "اگر محتوای اسناد پیوست‌شده یا قطعات بازیابی‌شده در ابتدای گفتگو آمده است، پاسخ را بر اساس همان اسناد بده " +
        "و اگر اطلاعات کافی نبود، صادقانه اعلام کن که در اسناد چیزی پیدا نکردی.";

    // سقف تاریخچه بارگذاری‌شده از دیتابیس برای هر درخواست
    private const int MaxHistoryMessages = 12;
    private const int MaxHistoryChars = 8000;

    private readonly Kernel _kernel = kernel;
    private readonly IDocumentStore _store = store;
    private readonly IConversationStore _conversations = conversations;
    private readonly IRagService _rag = rag;
    private readonly OllamaCapabilities _capabilities = capabilities;
    private readonly bool _useFunctionCalling = configuration.GetValue("Ollama:UseFunctionCalling", true);
    private readonly ILogger<ChatService> _logger = logger;

    private static readonly JsonSerializerOptions MetaJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    // ─────────────────────────────── پاسخ کامل ───────────────────────────────

    public async Task<ChatResponse> AskAsync(ChatRequest request, CancellationToken ct = default)
    {
        var conversation = _conversations.EnsureConversation(request.ConversationId, request.Message);
        var priorMessages = _conversations.GetMessages(conversation.Id, MaxHistoryMessages, MaxHistoryChars);

        var (history, docsUsed, ragUsed) = await BuildHistoryAsync(request, priorMessages);
        var chat = _kernel.GetRequiredService<IChatCompletionService>();

        var tools = await ResolveToolSupportAsync(ct);
        var settings = MakeSettings(tools);

        ChatMessageContent reply;
        try
        {
            reply = await chat.GetChatMessageContentAsync(history, settings, _kernel, ct);
        }
        catch (Exception ex) when (tools && ex.ToString().Contains("does not support tools", StringComparison.OrdinalIgnoreCase))
        {
            // مدل ابزارها را پشتیبانی نمی‌کند؛ بدون Function Calling دوباره تلاش می‌کنیم
            _logger.LogWarning("مدل از Function Calling پشتیبانی نمی‌کند؛ بدون ابزار دوباره تلاش می‌شود. خطا: {Message}", ex.Message);
            reply = await chat.GetChatMessageContentAsync(history, MakeSettings(false), _kernel, ct);
        }

        var (thinking, answer) = SplitThinking(reply.Content ?? string.Empty);

        // ذخیره پایا: پیام کاربر + پاسخ دستیار
        _conversations.AddMessage(conversation.Id, "user", request.Message, null);
        _conversations.AddMessage(conversation.Id, "assistant", answer, thinking);

        return new ChatResponse(answer, thinking, docsUsed, tools, ragUsed, conversation.Id, conversation.Title);
    }

    // ─────────────────────────────── پاسخ استریم ───────────────────────────────

    public async IAsyncEnumerable<StreamChunk> AskStreamAsync(
        ChatRequest request,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var conversation = _conversations.EnsureConversation(request.ConversationId, request.Message);
        var priorMessages = _conversations.GetMessages(conversation.Id, MaxHistoryMessages, MaxHistoryChars);

        var (history, docsUsed, ragUsed) = await BuildHistoryAsync(request, priorMessages);
        var chat = _kernel.GetRequiredService<IChatCompletionService>();

        var tools = await ResolveToolSupportAsync(ct);
        var settings = MakeSettings(tools);

        // رویداد اطلاعاتی اول: ابزارها، RAG و مشخصات مکالمه (JSON با camelCase)
        var meta = new StreamMeta(docsUsed, tools, ragUsed, conversation.Id, conversation.Title);
        yield return new StreamChunk("meta", JsonSerializer.Serialize(meta, MetaJsonOptions));

        /*
         * مدل DeepSeek-R1 ابتدا داخل تگ think «فکر می‌کند» و بعد جواب می‌دهد.
         * این State Machine کوچک، تگ‌ها را به‌صورت زنده از استریم جدا می‌کند.
         * تگ ممکن است وسط دو chunk نصف شود؛ به همین دلیل همیشه چند کاراکتر آخر
         * در بافر نگه داشته می‌شود.
         */
        var buffer = string.Empty;
        var insideThink = false;

        // برای ذخیره پایا، متن کامل هم در سمت سرور جمع می‌شود
        var answerText = new StringBuilder();
        var thinkText = new StringBuilder();

        await foreach (var update in chat.GetStreamingChatMessageContentsAsync(history, settings, _kernel, ct))
        {
            buffer += update.Content ?? string.Empty;
            if (buffer.Length == 0)
                continue;

            while (buffer.Length > 0)
            {
                if (!insideThink)
                {
                    var open = buffer.IndexOf("<think>", StringComparison.Ordinal);
                    if (open >= 0)
                    {
                        if (open > 0)
                        {
                            answerText.Append(buffer[..open]);
                            yield return new StreamChunk("answer", buffer[..open]);
                        }
                        buffer = buffer[(open + "<think>".Length)..];
                        insideThink = true;
                    }
                    else
                    {
                        var safe = buffer.Length - ("<think>".Length - 1);
                        if (safe > 0)
                        {
                            answerText.Append(buffer[..safe]);
                            yield return new StreamChunk("answer", buffer[..safe]);
                            buffer = buffer[safe..];
                        }
                        break;
                    }
                }
                else
                {
                    var close = buffer.IndexOf("</think>", StringComparison.Ordinal);
                    if (close >= 0)
                    {
                        if (close > 0)
                        {
                            thinkText.Append(buffer[..close]);
                            yield return new StreamChunk("thinking", buffer[..close]);
                        }
                        buffer = buffer[(close + "</think>".Length)..];
                        insideThink = false;
                    }
                    else
                    {
                        var safe = buffer.Length - ("</think>".Length - 1);
                        if (safe > 0)
                        {
                            thinkText.Append(buffer[..safe]);
                            yield return new StreamChunk("thinking", buffer[..safe]);
                            buffer = buffer[safe..];
                        }
                        break;
                    }
                }
            }
        }

        if (buffer.Length > 0)
        {
            yield return new StreamChunk(insideThink ? "thinking" : "answer", buffer);
            (insideThink ? thinkText : answerText).Append(buffer);
        }

        // ذخیره پایا قبل از پایان استریم
        _conversations.AddMessage(conversation.Id, "user", request.Message, null);
        _conversations.AddMessage(conversation.Id, "assistant", answerText.ToString().Trim(), TrimOrNull(thinkText.ToString()));

        yield return new StreamChunk("done", string.Empty);
    }

    // ─────────────────────────────── متدهای کمکی ───────────────────────────────

    /// <summary>
    /// ساخت ChatHistory به ترتیب: پیام سیستم ← زمینه اسناد (RAG یا متن کامل) ←
    /// تاریخچه قبلی مکالمه ← پیام فعلی کاربر.
    /// </summary>
    private async Task<(ChatHistory History, int DocsUsed, bool RagUsed)> BuildHistoryAsync(
        ChatRequest request,
        IReadOnlyList<ConversationMessage> priorMessages)
    {
        var history = new ChatHistory();
        history.AddSystemMessage(SystemPrompt);

        var docsUsed = 0;
        var ragUsed = false;

        // ۱) زمینه اسناد پیوست
        if (request.DocumentIds is { Length: > 0 })
        {
            // اول RAG را امتحان می‌کنیم (بازیابی معنایی قطعات مرتبط با سؤال)
            var (used, context) = await _rag.BuildContextAsync(request.Message, request.DocumentIds);

            if (used)
            {
                history.AddUserMessage(context);
                ragUsed = true;
                docsUsed = request.DocumentIds.Length;
            }
            else
            {
                // Fallback: تزریق متن کامل اسناد (کوتاه‌شده) مثل نسخه اولیه
                var sb = new StringBuilder("### اسناد پیوست‌شده توسط کاربر:\n");
                foreach (var id in request.DocumentIds)
                {
                    var doc = _store.Get(id);
                    if (doc is null)
                        continue;

                    sb.AppendLine($"\n--- سند: «{doc.FileName}» ({doc.FileKind}) ---");
                    sb.AppendLine(doc.Content);
                    docsUsed++;
                }

                if (docsUsed > 0)
                    history.AddUserMessage(sb.ToString());
            }
        }

        // ۲) تاریخچه قبلی گفتگو (حافظه پایا)
        foreach (var message in priorMessages)
        {
            if (string.IsNullOrWhiteSpace(message.Content))
                continue;

            if (message.Role == "assistant")
                history.AddAssistantMessage(message.Content);
            else
                history.AddUserMessage(message.Content);
        }

        // ۳) پیام فعلی کاربر
        history.AddUserMessage(request.Message);
        return (history, docsUsed, ragUsed);
    }

    /// <summary>تنظیمات اجرا: فعال یا غیرفعال کردن Function Calling خودکار</summary>
    private static OllamaPromptExecutionSettings MakeSettings(bool tools) => new()
    {
        FunctionChoiceBehavior = tools ? FunctionChoiceBehavior.Auto() : FunctionChoiceBehavior.None()
    };

    private async Task<bool> ResolveToolSupportAsync(CancellationToken ct)
    {
        if (!_useFunctionCalling)
            return false;

        return await _capabilities.SupportsToolCallingAsync(ct);
    }

    /// <summary>جداسازی متن تگ‌های think از پاسخ نهایی (برای حالت غیراستریم)</summary>
    private static (string? Thinking, string Answer) SplitThinking(string content)
    {
        var match = Regex.Match(content, "<think>([\\s\\S]*?)</think>");
        if (!match.Success)
            return (null, content.Trim());

        var thinking = match.Groups[1].Value.Trim();
        var answer = content.Remove(match.Index, match.Length).Trim();
        return (string.IsNullOrWhiteSpace(thinking) ? null : thinking, answer);
    }

    private static string? TrimOrNull(string value)
    {
        var trimmed = value.Trim();
        return trimmed.Length == 0 ? null : trimmed;
    }
}
