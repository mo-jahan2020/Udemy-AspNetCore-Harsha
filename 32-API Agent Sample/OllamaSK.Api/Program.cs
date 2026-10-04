using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Ollama;
using OllamaSK.Api.Filters;
using OllamaSK.Api.Models;
using OllamaSK.Api.Plugins;
using OllamaSK.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────── Controller + JSON ───────────────────────────
builder.Services.AddControllers().AddJsonOptions(o =>
{
    // نمایش درست حروف فارسی در خروجی JSON (به‌جای \u0627)
    o.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
});

// ─────────────────────────── OpenAPI (اسکیمای JSON) ───────────────────────────
builder.Services.AddOpenApi();

// ─────────────────────────── خواندن تنظیمات Ollama ───────────────────────────
var ollamaEndpoint = builder.Configuration["Ollama:Endpoint"] ?? "http://localhost:11434";
var ollamaModelId = builder.Configuration["Ollama:ModelId"] ?? "deepseek-r1:7b";

/*
 * ───────────────────── مرحله ۶ و ۷: Semantic Kernel + Ollama ─────────────────────
 * کانکتور رسمی «Microsoft.SemanticKernel.Connectors.Ollama» روی پکیج OllamaSharp
 * ساخته شده و IChatCompletionService آماده به Kernel اضافه می‌کند.
 * سپس دو Plugin (ماشین‌حساب و جستجو) برای Function Calling خودکار ثبت می‌شوند.
 */
var kernelBuilder = builder.Services
    .AddKernel()
    .AddOllamaChatCompletion(
        modelId: ollamaModelId,
        endpoint: new Uri(ollamaEndpoint));

// ثبت دو Plugin (ماشین‌حساب و جستجو) برای Function Calling خودکار
kernelBuilder.Plugins.AddFromType<CalculatorPlugin>("Calculator");
kernelBuilder.Plugins.AddFromType<SearchPlugin>("Search");

// HttpClient مشترک برای بررسی وضعیت Ollama (GET /api/tags)
builder.Services.AddHttpClient("ollama", c => c.BaseAddress = new Uri(ollamaEndpoint));

// SearchPlugin به HttpClient نیاز دارد (الگوی Typed Client)
builder.Services.AddHttpClient<SearchPlugin>();

// CalculatorPlugin در DI ثبت می‌شود تا Kernel و Controller ها هر دو بتوانند از آن استفاده کنند
builder.Services.AddSingleton<CalculatorPlugin>();

// فیلتر لاگ فراخوانی Plugin‌ها — در کنسول نشان می‌دهد مدل کدام ابزار را صدا زده است
builder.Services.AddSingleton<IFunctionInvocationFilter, PluginLoggingFilter>();

// ─────────────────────────── دیتابیس SQLite (پایا) ───────────────────────────
// یک فایل ساده بدون نیاز به سرور؛ تاریخچه مکالمات، اسناد و وکتورهای RAG در آن می‌مانند
builder.Services.AddSingleton<SqliteConnectionFactory>();

// ─────────────────────────── سرویس‌های برنامه ───────────────────────────
builder.Services.Configure<FileProcessingOptions>(builder.Configuration.GetSection("FileProcessing"));
builder.Services.Configure<RagOptions>(builder.Configuration.GetSection("Rag"));

// ذخیره‌گاه‌های پایا (SQLite)
builder.Services.AddSingleton<IDocumentStore, DocumentStore>();
builder.Services.AddSingleton<IConversationStore, ConversationStore>();
builder.Services.AddSingleton<IVectorStore, SqliteVectorStore>();

// استخراج متن فایل‌ها (متنی/ورد/اکسل)
builder.Services.AddSingleton<IDocumentParser, DocumentParser>();

// RAG: مدل Embedding + سرویس بازیابی معنایی
builder.Services.AddSingleton<IEmbeddingService, EmbeddingService>();
builder.Services.AddSingleton<IRagService, RagService>();

builder.Services.AddSingleton<OllamaCapabilities>();
builder.Services.AddScoped<IChatService, ChatService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // اسکیمای OpenAPI روی /openapi/v1.json
}

app.UseDefaultFiles();  // سرو کردن wwwroot/index.html به‌عنوان صفحه پیش‌فرض
app.UseStaticFiles();   // سرو کردن فایل‌های استاتیک (صفحه تست فارسی)

app.MapControllers();

app.Run();
