# 🤖 دستیار هوشمند — ASP.NET Core 10 + Semantic Kernel + Ollama

نمونه‌کد کامل ارتباط یک API با مدل محلی **DeepSeek-R1** از طریق **Semantic Kernel** و پکیج **OllamaSharp**، به‌همراه:

| قابلیت | توضیح |
|---|---|
| 💬 چت استریم (SSE) | پاسخ زنده + جداسازی «تفکر مدل» DeepSeek-R1 |
| 🗂 تاریخچه پایا | مکالمات و پیام‌ها در **SQLite** — بعد از ری‌استارت هم می‌مانند |
| 🧠 RAG | آپلود سند → قطعه‌بندی → **Embedding** با `nomic-embed-text` → جستجوی شباهت کسینوسی در SQLite → تزریق قطعات مرتبط در پرامپت |
| 📎 پیوست فایل | TXT / Word (docx) / Excel (xlsx) با استخراج خودکار |
| 🔧 دو Plugin واقعی | ماشین‌حساب (۵ تابع) + جستجوی وب با موتور ایرانی **پارسیجو**، ویکی‌پدیای فارسی و DuckDuckGo |

---

## ✅ پیش‌نیازها

```bash
ollama list          # باید deepseek-r1:7b در لیست باشد
curl http://localhost:11434/api/tags

# برای فعال شدن RAG (پیشنهادی):
ollama pull nomic-embed-text
```

## 📦 پکیج‌های استفاده‌شده

| پکیج | کاربرد |
|---|---|
| `Microsoft.SemanticKernel` 1.80.1 | هسته SK |
| `Microsoft.SemanticKernel.Connectors.Ollama` 1.80.1-alpha | کانکتور رسمی Ollama (روی OllamaSharp) |
| `OllamaSharp` 5.4.30 | چت + تولید Embedding |
| `Microsoft.Data.Sqlite` 10.0.11 | دیتابیس پایا (مکالمات + اسناد + وکتورها) |
| `DocumentFormat.OpenXml` 3.5.1 | خواندن فایل ورد |
| `ClosedXML` 0.105.1 | خواندن فایل اکسل |
| `Microsoft.AspNetCore.OpenApi` 10.0.11 | اسکیمای OpenAPI |

## 🗂 ساختار پروژه

```
OllamaSK.Api/
├── OllamaSK.Api.csproj
├── Program.cs                   ← ثبت SK، Plugin‌ها، SQLite و سرویس‌های RAG
├── appsettings.json             ← Ollama، Search، Rag، Database، FileProcessing
├── Properties/launchSettings.json   (پورت 5277)
├── Controllers/
│   ├── ChatController.cs        ← POST /api/chat و /api/chat/stream (SSE)
│   ├── ConversationsController.cs ← تاریخچه مکالمات (لیست/پیام‌ها/تغییر نام/حذف)
│   ├── FilesController.cs       ← آپلود فایل + ایندکس خودکار RAG
│   ├── RagController.cs         ← وضعیت RAG، جستجوی معنایی مستقیم، ایندکس مجدد
│   ├── ToolsController.cs       ← فراخوانی مستقیم Plugin‌ها
│   └── OllamaController.cs      ← Health + تولید متن مستقیم با OllamaSharp
├── Services/
│   ├── ChatService.cs           ← چت + استریم + تاریخچه + RAG + جداسازی <think>
│   ├── ConversationStore.cs     ← تاریخچه پایا در SQLite
│   ├── DocumentStore.cs         ← اسناد پایا در SQLite
│   ├── DocumentParser.cs        ← استخراج متن از ورد/اکسل/متنی
│   ├── EmbeddingService.cs      ← Embedding با OllamaSharp (مدل nomic-embed-text)
│   ├── VectorStore.cs           ← Vector Store روی SQLite + شباهت کسینوسی
│   ├── RagService.cs            ← قطعه‌بندی + ایندکس + ساخت زمینه پرامپت
│   └── OllamaCapabilities.cs    ← تشخیص پشتیبانی مدل از Function Calling
├── Plugins/
│   ├── CalculatorPlugin.cs      ← ۵ تابع ریاضی
│   └── SearchPlugin.cs          ← Parsijoo / Wikipedia / DuckDuckGo / Auto
├── Filters/PluginLoggingFilter.cs
└── wwwroot/index.html           ← صفحه تست فارسی (سایدبار مکالمات + RAG + ابزارها)
```

## ▶️ اجرا

```bash
dotnet restore
dotnet run
```

سپس مرورگر: **http://localhost:5277**

## 🧠 RAG چطور کار می‌کند؟

1. **آپلود سند**: متن استخراج، به قطعات ~۹۰۰ کاراکتری (با هم‌پوشانی ۱۵۰) تقسیم می‌شود.
2. هر قطعه با مدل `nomic-embed-text` به وکتور تبدیل و در جدول `chunks` (SQLite، BLOB) ذخیره می‌شود.
3. **هر سؤال چت** با اسناد انتخابی: سؤال Embed می‌شود → شباهت کسینوسی با همه قطعات → ۴ قطعه برتر در پرامپت تزریق می‌شود.
4. اگر مدل Embedding نصب نباشد: برنامه به‌صورت خودکار متن کامل سند (کوتاه‌شده) را می‌فرستد — همان رفتار نسخه قبل. بعد از نصب مدل، دکمه «ایندکس مجدد» یا `POST /api/rag/reindex` را بزنید.
5. وکتورها در `App_Data/ollamask.db` می‌مانند و بعد از ری‌استارت هم معتبرند.

## 🗂 تاریخچه مکالمات

- هر پیام (کاربر + پاسخ دستیار + متن تفکر) در SQLite ذخیره می‌شود.
- در هر درخواست، آخرین ~۱۲ پیام (سقف ۸۰۰۰ کاراکتر) به ChatHistory اضافه می‌شود.
- `POST /api/chat` بدون `conversationId` → مکالمه جدید با عنوان خودکار؛ با `conversationId` → ادامه همان گفتگو.
- سایدبار صفحه: بازکردن گفتگوی قبلی، حذف، مکالمه جدید.

## 🔍 جستجوی وب

```json
"Search": { "Provider": "Auto", "MaxResults": 5 }
```

- `Auto` (پیش‌فرض): پارسیجو ← ویکی‌پدیای فارسی ← DuckDuckGo (اولین موفق)
- `Parsijoo` / `Wikipedia` / `DuckDuckGo`: اجبار به یک ارائه‌دهنده
- در endpoint مستقیم هم قابل انتخاب است: `GET /api/tools/search?q=...&provider=Parsijoo`

## 🧪 تست سریع با curl

```bash
# وضعیت Ollama
curl http://localhost:5277/api/ollama/health

# وضعیت RAG (مدل Embedding + تعداد قطعات)
curl http://localhost:5277/api/rag/status

# آپلود فایل (ایندکس RAG خودکار)
curl -F "file=@report.docx" http://localhost:5277/api/files/upload

# جستجوی معنایی مستقیم در اسناد
curl "http://localhost:5277/api/rag/search?q=سؤال شما&topK=3"

# چت با حافظه (اولین پیام — مکالمه جدید)
curl -X POST http://localhost:5277/api/chat \
  -H "Content-Type: application/json" \
  -d '{"message":"سلام! اسم من علی است"}'

# ادامه همان مکالمه (conversationId را از پاسخ قبلی بردارید)
curl -X POST http://localhost:5277/api/chat \
  -H "Content-Type: application/json" \
  -d '{"message":"اسم من چه بود؟","conversationId":"GUID-از-پاسخ-قبلی"}'

# لیست مکالمات و پیام‌ها
curl http://localhost:5277/api/conversations
curl http://localhost:5277/api/conversations/{id}/messages

# جستجوی وب با موتور ایرانی
curl "http://localhost:5277/api/tools/search?q=%D8%AF%D8%A7%D9%86%D8%B4%DA%AF%D8%A7%D9%87&provider=Parsijoo"

# فراخوانی مستقیم ماشین‌حساب
curl -X POST http://localhost:5277/api/tools/calculate \
  -H "Content-Type: application/json" \
  -d '{"operation":"multiply","a":12,"b":7}'
```

## 🔧 عیب‌یابی

| مشکل | راه‌حل |
|---|---|
| `does not support tools` در لاگ | deepseek-r1 ابزار ندارد؛ برنامه خودکار بدون ابزار ادامه می‌دهد. برای Function Calling واقعی: `ollama pull qwen2.5:7b` و تغییر `Ollama:ModelId` |
| RAG غیرفعال است | `ollama pull nomic-embed-text` را بزنید، بعد «ایندکس مجدد» در صفحه یا `POST /api/rag/reindex` |
| جستجوی پارسیجو جواب نداد | ممکن است ساختار HTML تغییر کرده باشد؛ Auto سراغ ویکی‌پدیا می‌رود. Provider را در appsettings ثابت کنید |
| صفحه بالا نیامد | `ollama serve` چک شود؛ پورت با `dotnet run --urls http://localhost:5000` تغییر می‌کند |
| فایل doc/xls قبول نشد | فقط docx و xlsx پشتیبانی می‌شود (Save As کنید) |
| دیتابیس را می‌خواهم صفر کنم | `App_Data/ollamask.db` را حذف کنید (یا DELETE /api/conversations) |

## ⚙️ تنظیمات کامل appsettings.json

```json
"Ollama": {
  "Endpoint": "http://localhost:11434",
  "ModelId": "deepseek-r1:7b",
  "EmbeddingModelId": "nomic-embed-text",
  "UseFunctionCalling": true
},
"Search": { "Provider": "Auto", "MaxResults": 5 },
"Rag": { "ChunkSize": 900, "ChunkOverlap": 150, "TopK": 4, "MaxContextChars": 4500 },
"Database": { "FileName": "App_Data/ollamask.db" },
"FileProcessing": { "MaxFileSizeMB": 10, "MaxCharsPerDocument": 12000 }
```

## 📡 API ها در یک نگاه

| متد | مسیر | کار |
|---|---|---|
| GET | `/api/ollama/health` | وضعیت Ollama و لیست مدل‌ها |
| POST | `/api/chat` | چت (پاسخ کامل JSON + ذخیره در تاریخچه) |
| POST | `/api/chat/stream` | چت استریم (SSE) با جداسازی تفکر |
| GET/POST | `/api/conversations` | لیست / ساخت مکالمه |
| GET/PATCH/DELETE | `/api/conversations/{id}` | پیام‌ها / تغییر عنوان / حذف |
| POST | `/api/files/upload` | آپلود txt/docx/xlsx + ایندکس RAG |
| GET/DELETE | `/api/files` | لیست / حذف اسناد |
| GET | `/api/rag/status` | وضعیت RAG و مدل Embedding |
| GET | `/api/rag/search?q=` | جستجوی معنایی مستقیم |
| POST | `/api/rag/reindex` | ایندکس مجدد همه اسناد |
| GET | `/api/tools` | لیست Plugin‌های Kernel |
| POST | `/api/tools/calculate` | ماشین‌حساب مستقیم |
| GET | `/api/tools/search?q=&provider=` | جستجوی وب (پارسیجو/ویکی‌پدیا/DDG) |
