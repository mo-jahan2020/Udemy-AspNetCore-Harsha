# 🧠 سرویس Text-to-SQL (ASP.NET Core 10 + Python + Ollama + Vanna.ai)

> یک سرویس کامل Web API برای تبدیل پرامپت زبانی کاربر به Query حرفه‌ای SQL Server با استفاده از هوش مصنوعی محلی.

---

## 📋 فهرست مطالب

1. [معماری کلی](#-معماری-کلی)
2. [پیش‌نیازها](#-پیش‌نیازها)
3. [نصب Ollama و دانلود مدل](#-نصب-ollama-و-دانلود-مدل)
4. [نصب پیش‌نیازهای Python](#-نصب-پیش‌نیازهای-python)
5. [تنظیم Connection String](#-تنظیم-connection-string)
6. [اجرای سرویس‌ها](#-اجرای-سرویس‌ها)
7. [مستندسازی Swagger](#-مستندسازی-swagger)
8. [نمونه تست با curl](#-نمونه-تست-با-curl)
9. [Health Check](#-health-check)
10. [فرض‌های منطقی اعمال‌شده](#-فرض‌های-منطقی-اعمالشده)
11. [ساختار پروژه](#-ساختار-پروژه)
12. [عیب‌یابی](#-عیب‌یابی)

---

## 🏗 معماری کلی

سرویس از سه لایه مجزا تشکیل شده است:

```
┌──────────────────────────────────────────────────────────────────┐
│                    کاربر / Client (curl, Postman)                  │
└──────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌──────────────────────────────────────────────────────────────────┐
│                  سرویس اصلی: ASP.NET Core 10 Web API              │
│                       (Port: 5000, C# / .NET 10)                 │
│                                                                  │
│   Controllers  →  Services  →  Repositories  →  EF Core          │
│   - TextToSql     - Ollama      - Session        - SQL Server     │
│   - Sessions      - Vanna       - Repository     - Express       │
│   - Health        - Sql                                          │
│                   - Session                                      │
└──────────────────────────────────────────────────────────────────┘
         │                          │                    │
         ▼                          ▼                    ▼
┌────────────────┐         ┌────────────────┐   ┌──────────────────┐
│   SQL Server   │         │  Vanna Python  │   │  دیتابیس Session │
│   (دیتابیس هدف) │         │  (FastAPI)     │   │  (SQL Express)   │
│   - SampleDb   │         │  Port: 8001    │   │  - Sessions      │
│                │         └────────────────┘   │  - SessionMsgs   │
└────────────────┘                  │           └──────────────────┘
                                    ▼
                            ┌────────────────┐
                            │    Ollama      │
                            │ Port: 11434    │
                            │ qwen2.5-coder  │
                            └────────────────┘
```

### جریان تولید SQL

1. کاربر درخواست `POST /api/texttosql/generate` با `{ sessionId, prompt }` می‌فرستد.
2. سرویس `TextToSqlService` ابتدا پیام کاربر را در Session ذخیره می‌کند.
3. سپس به میکروسرویس Vanna درخواست می‌دهد.
4. اگر Vanna در دسترس نباشد و `EnableFallbackToOllama=true` باشد، مستقیماً با Ollama و تزریق شمای دیتابیس در System Prompt تولید SQL انجام می‌شود.
5. SQL نهایی به همراه زمان پاسخ و نام منبع (Vanna / Ollama-Fallback) به کاربر برمی‌گردد.
6. پیام دستیار (SQL تولیدشده) نیز در Session ذخیره می‌شود.

---

## ✅ پیش‌نیازها

| پیش‌نیاز | حداقل نسخه | توضیح |
|---|---|---|
| [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) | 10.0.0 | برای build و اجرای سرویس اصلی |
| [Python](https://www.python.org/downloads/) | 3.11+ | برای میکروسرویس Vanna |
| [Ollama](https://ollama.com/download) | آخرین نسخه | برای اجرای مدل محلی |
| [SQL Server Express](https://www.microsoft.com/sql-server/sql-server-downloads) | 2019+ | دیتابیس هدف و دیتابیس سشن |
| [Microsoft ODBC Driver 18 for SQL Server](https://learn.microsoft.com/sql/connect/odbc/download-odbc-driver-for-sql-server) | 18.x | برای اتصال Python به SQL Server (در Docker خودکار نصب می‌شود) |
| [Docker](https://www.docker.com/) (اختیاری) | 24+ | برای اجرای کل مجموعه با docker-compose |

---

## 🦙 نصب Ollama و دانلود مدل

### روی ویندوز / macOS

1. از [سایت رسمی Ollama](https://ollama.com/download) نسخه مناسب سیستم‌عامل خود را دانلود و نصب کنید.
2. پس از نصب، Ollama به‌صورت خودکار روی پورت `11434` اجرا می‌شود.
3. مدل را دانلود کنید (حدود 4GB):
   ```bash
   ollama pull qwen2.5-coder
   ```
4. (اختیاری) تست مدل:
   ```bash
   ollama run qwen2.5-coder "SELECT TOP 10 * FROM Customers"
   ```

### روی Linux (Ubuntu/Debian)

```bash
# نصب Ollama
curl -fsSL https://ollama.com/install.sh | sh

# دانلود مدل
ollama pull qwen2.5-coder
```

### مدل‌های پیشنهادی جایگزین

| مدل | حجم | کیفیت | مناسب برای |
|---|---|---|---|
| `qwen2.5-coder` | ~4GB | ⭐⭐⭐⭐⭐ | بهترین انتخاب برای SQL |
| `codellama` | ~4GB | ⭐⭐⭐⭐ | خوب برای SQL |
| `llama3.1` | ~5GB | ⭐⭐⭐⭐ | عمومی |
| `deepseek-coder-v2` | ~6GB | ⭐⭐⭐⭐ | کدنویسی |

برای تغییر مدل، در `appsettings.json` بخش `Ollama:Model` را به‌روزرسانی کنید.

---

## 🐍 نصب پیش‌نیازهای Python

### روی سیستم محلی

```bash
cd python-vanna-service

# (پیشنهاد) ساخت محیط مجازی
python -m venv venv
# روی Linux/macOS:
source venv/bin/activate
# روی Windows:
venv\Scripts\activate

# نصب وابستگی‌ها
pip install -r requirements.txt
```

### نصب ODBC Driver 18 (برای Linux)

```bash
# Ubuntu/Debian
sudo su
curl https://packages.microsoft.com/keys/microsoft.asc | apt-key add -
curl https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/prod.list > /etc/apt/sources.list.d/mssql-release.list
exit
sudo apt-get update
sudo ACCEPT_EULA=Y apt-get install -y msodbcsql18
sudo apt-get install -y unixodbc-dev
```

### اجرای سرویس Vanna

```bash
cd python-vanna-service
cp .env.example .env
# ویرایش .env و تنظیم OLLAMA_BASE_URL و مدل
python main.py
# یا:
uvicorn main:app --host 0.0.0.0 --port 8001 --reload
```

سرویس روی `http://localhost:8001` اجرا می‌شود.
مستندات Swagger: `http://localhost:8001/docs`

---

## 🔗 تنظیم Connection String

در فایل `src/TextToSqlApi/src/TextToSqlApi/appsettings.json` دو Connection String وجود دارد:

```json
"ConnectionStrings": {
  "TargetDatabase": "Server=localhost\\SQLEXPRESS;Database=SampleDb;Trusted_Connection=True;TrustServerCertificate=True;",
  "SessionDatabase": "Server=localhost\\SQLEXPRESS;Database=TextToSqlSessions;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

| Connection String | کاربرد |
|---|---|
| `TargetDatabase` | دیتابیس هدف کاربر که SQL تولیدشده روی آن اجرا می‌شود |
| `SessionDatabase` | دیتابیس محلی برای ذخیره تاریخچه چت کاربران |

### نمونه Connection String با احراز هویت SQL

```text
Server=myserver.database.windows.net;Database=MyDb;User Id=myuser;Password=MyPassword123!;Encrypt=True;TrustServerCertificate=False;
```

### Override با Environment Variables

```bash
# روی Linux/macOS
export ConnectionStrings__TargetDatabase="Server=...;Database=...;"
export ConnectionStrings__SessionDatabase="Server=...;Database=...;"

# روی Windows (PowerShell)
$env:ConnectionStrings__TargetDatabase="Server=...;Database=...;"
$env:ConnectionStrings__SessionDatabase="Server=...;Database=...;"
```

---

## 🚀 اجرای سرویس‌ها

### روش ۱: اجرای کامل با Docker Compose (توصیه‌شده)

```bash
# از ریشه پروژه
docker-compose up -d

# مشاهده لاگ‌ها
docker-compose logs -f

# بررسی وضعیت
docker-compose ps

# توقف
docker-compose down
```

این دستور سرویس‌های زیر را اجرا می‌کند:
- **sqlserver**: SQL Server 2022 Express روی پورت `1433`
- **ollama**: Ollama روی پورت `11434`
- **ollama-init**: یکبار مدل `qwen2.5-coder` را دانلود می‌کند
- **vanna-service**: میکروسرویس Python روی پورت `8001`
- **texttosql-api**: Web API روی پورت `5000`

### روش ۲: اجرای دستی (بدون Docker)

#### گام ۱: اجرای Ollama

```bash
# Ollama را نصب و اجرا کنید (در پس‌زمینه)
ollama serve
# در یک ترمینال دیگر:
ollama pull qwen2.5-coder
```

#### گام ۲: اجرای میکروسرویس Vanna

```bash
cd python-vanna-service
cp .env.example .env
pip install -r requirements.txt
python main.py
```

#### گام ۳: اجرای اسکریپت‌های SQL

در SQL Server Management Studio (SSMS) یا با `sqlcmd`، اسکریپت‌های زیر را اجرا کنید:

```bash
sqlcmd -S localhost\SQLEXPRESS -E -i sql/01_create_session_tables.sql
sqlcmd -S localhost\SQLEXPRESS -E -i sql/02_create_sample_target_db.sql
```

#### گام ۴: اجرای Web API

```bash
cd src/TextToSqlApi
dotnet restore
dotnet build
dotnet run --project src/TextToSqlApi
```

سرویس روی `http://localhost:5000` اجرا می‌شود.

#### گام ۵: (اختیاری) استفاده از EF Core Migrations

اگر به‌جای اسکریپت SQL، می‌خواهید از Migration استفاده کنید:

```bash
cd src/TextToSqlApi
# نصب ابزار EF (یکبار)
dotnet tool install --global dotnet-ef

# ایجاد Migration اولیه (اگر از قبل وجود نداشت)
dotnet ef migrations add InitialCreate --project src/TextToSqlApi --startup-project src/TextToSqlApi

# اعمال Migration روی دیتابیس
dotnet ef database update --project src/TextToSqlApi --startup-project src/TextToSqlApi
```

> **نکته**: در `appsettings.json` اگر `App:ApplyMigrationsOnStartup=true` باشد، در زمان استارتاپ سرویس، Migrationها به‌صورت خودکار اعمال می‌شوند.

---

## 📚 مستندسازی Swagger

پس از اجرای سرویس، مستندات تعاملی Swagger در آدرس‌های زیر در دسترس است:

- **Swagger UI**: `http://localhost:5000/swagger`
- **OpenAPI JSON**: `http://localhost:5000/swagger/v1/swagger.json`

تمام Endpointها با schema کامل، توضیحات، و نمونه‌های ورودی/خروجی مستند شده‌اند.

---

## 🧪 نمونه تست با curl

### ۱. ایجاد یک نشست جدید

```bash
curl -X POST http://localhost:5000/api/sessions \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "ali",
    "title": "Test session"
  }'
```

**پاسخ نمونه**:
```json
{
  "id": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "userName": "ali",
  "title": "Test session",
  "createdAt": "2024-12-01T10:00:00Z",
  "updatedAt": "2024-12-01T10:00:00Z",
  "messageCount": 0
}
```

### ۲. ثبت شما دیتابیس (DDL جداول)

```bash
curl -X POST http://localhost:5000/api/texttosql/schema \
  -H "Content-Type: application/json" \
  -d '{
    "schemaText": "Tables:\n- Customers(CustomerId INT PK, FirstName NVARCHAR(100), LastName NVARCHAR(100), Email NVARCHAR(200), Country NVARCHAR(100))\n- Products(ProductId INT PK, ProductName NVARCHAR(200), Category NVARCHAR(100), UnitPrice DECIMAL(18,2), StockQuantity INT)\n- Orders(OrderId INT PK, CustomerId INT FK->Customers, OrderDate DATETIME2, TotalAmount DECIMAL(18,2))\n- OrderItems(OrderItemId INT PK, OrderId INT FK->Orders, ProductId INT FK->Products, Quantity INT, UnitPrice DECIMAL(18,2))\n\nSample data:\n- 5 customers from Iran, USA, China, UK\n- 5 products in Electronics, Furniture, Books, Accessories\n- 5 orders between Jan-May 2024"
  }'
```

### ۳. تولید SQL از پرامپت

```bash
# SESSION_ID را از پاسخ گام ۱ جایگزین کنید
SESSION_ID="f47ac10b-58cc-4372-a567-0e02b2c3d479"

curl -X POST http://localhost:5000/api/texttosql/generate \
  -H "Content-Type: application/json" \
  -d "{
    \"sessionId\": \"$SESSION_ID\",
    \"prompt\": \"فروش ماهانه سال 2024 را نشان بده\"
  }"
```

**پاسخ نمونه**:
```json
{
  "sql": "SELECT FORMAT(o.OrderDate, 'yyyy-MM') AS Month, SUM(o.TotalAmount) AS TotalSales FROM Orders o WHERE o.OrderDate >= '2024-01-01' AND o.OrderDate < '2025-01-01' GROUP BY FORMAT(o.OrderDate, 'yyyy-MM') ORDER BY Month",
  "sessionId": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "responseTimeMs": 2340,
  "modelName": "Vanna",
  "source": "Vanna"
}
```

### ۴. اجرای SQL تولیدشده (فقط Read-Only)

```bash
curl -X POST http://localhost:5000/api/texttosql/execute \
  -H "Content-Type: application/json" \
  -d '{
    "sql": "SELECT TOP 10 * FROM Customers",
    "maxRows": 10
  }'
```

**پاسخ نمونه**:
```json
{
  "columns": ["CustomerId", "FirstName", "LastName", "Email", "Country", "CreatedAt"],
  "rows": [
    {"CustomerId": 1, "FirstName": "علی", "LastName": "محمدی", "Email": "ali@example.com", "Country": "Iran", "CreatedAt": "2024-01-01T00:00:00"},
    {"CustomerId": 2, "FirstName": "Mary", "LastName": "Johnson", "Email": "mary@example.com", "Country": "USA", "CreatedAt": "2024-01-01T00:00:00"}
  ],
  "rowCount": 2,
  "executionTimeMs": 45
}
```

### ۵. تست بلاک دستور مخرب

```bash
curl -X POST http://localhost:5000/api/texttosql/execute \
  -H "Content-Type: application/json" \
  -d '{
    "sql": "DROP TABLE Customers"
  }'
```

**پاسخ (HTTP 400)**:
```json
{
  "type": "https://httpstatuses.io/400",
  "title": "Bad request",
  "status": 400,
  "detail": "Blocked: keyword 'DROP' is not allowed in read-only mode.",
  "instance": "/api/texttosql/execute",
  "traceId": "00-abc123..."
}
```

### ۶. دریافت تاریخچه یک نشست

```bash
SESSION_ID="f47ac10b-58cc-4372-a567-0e02b2c3d479"

curl http://localhost:5000/api/sessions/$SESSION_ID
```

### ۷. لیست نشست‌های یک کاربر

```bash
curl "http://localhost:5000/api/sessions?userName=ali"
```

### ۸. حذف یک نشست

```bash
SESSION_ID="f47ac10b-58cc-4372-a567-0e02b2c3d479"

curl -X DELETE http://localhost:5000/api/sessions/$SESSION_ID
```

### ۹. اجرای Unit Testها

#### .NET Tests
```bash
cd src/TextToSqlApi
dotnet test
```

#### Python Tests
```bash
cd python-vanna-service
pip install -r requirements-dev.txt
pytest -v
```

---

## ❤️ Health Check

### Endpoint اختصاصی

```bash
curl http://localhost:5000/api/health
```

**پاسخ نمونه**:
```json
{
  "status": "Healthy",
  "timestamp": "2024-12-01T10:00:00Z",
  "components": {
    "Ollama": "Healthy",
    "Vanna": "Healthy",
    "TargetDatabase": "Healthy"
  }
}
```

### Endpoint سبک ASP.NET Core

```bash
curl http://localhost:5000/health
```

### Vanna Microservice Health

```bash
curl http://localhost:8001/health
```

---

## 🤔 فرض‌های منطقی اعمال‌شده

طبق درخواست شما، در موارد مبهم فرض‌های منطقی زیر اعمال شده‌اند:

1. **مدل پیش‌فرض**: `qwen2.5-coder` به‌عنوان بهترین مدل برای SQL انتخاب شده. قابل تغییر در `appsettings.json`.
2. **پورت‌ها**: Web API روی `5000`، Vanna روی `8001`، Ollama روی `11434`، SQL Server روی `1433`.
3. **دیتابیس سشن**: نام `TextToSqlSessions` با اسکیما در فایل `sql/01_create_session_tables.sql`.
4. **Fallback به Ollama**: در صورت در دسترس نبودن Vanna، به‌صورت پیش‌فرض فعال است (`EnableFallbackToOllama=true`).
5. **System Prompt**: شامل قواعد سخت‌گیرانه Read-Only (فقط SELECT، بدون INSERT/UPDATE/DELETE/DROP/...).
6. **اعتبارسنجی SQL**: با Regex و لیست سیاه کلمات کلیدی انجام می‌شود. کامنت‌های SQL قبل از اعتبارسنجی حذف می‌شوند.
7. **MaxRows پیش‌فرض**: 100 ردیف برای اجرای SQL.
8. **بالا آمدن مجدد Migration**: `ApplyMigrationsOnStartup=true` در محیط Production باید `false` شود.
9. **CORS**: در صورت تنظیم نشدن `App:CorsOrigins`، تمام Originها مجاز می‌شوند.
10. **استراتژی ذخیره شما**: در حافظه سرویس نگه‌داری می‌شود (فرض: تک‌نودی). در Production باید در Redis یا cache توزیع‌شده ذخیره شود.
11. **احراز هویت**: در این نسخه پیاده‌سازی نشده. فرض: سرویس در شبکه داخلی یا پشت API Gateway قرار دارد.
12. **Log Files**: در پوشه `logs/` سرویس .NET ذخیره می‌شوند.

---

## 📁 ساختار پروژه

```
text-to-sql-service/
├── README.md                                  # این فایل
├── docker-compose.yml                         # اجرای کل مجموعه
├── .env.example                               # نمونه Environment Variables
│
├── sql/                                       # اسکریپت‌های SQL Server
│   ├── 01_create_session_tables.sql           # ایجاد جداول Sessions و SessionMessages
│   └── 02_create_sample_target_db.sql         # دیتابیس نمونه برای تست
│
├── src/TextToSqlApi/                          # سرویس اصلی .NET 10
│   ├── TextToSqlApi.sln                       # Solution file
│   ├── src/TextToSqlApi/
│   │   ├── TextToSqlApi.csproj                # پروژه اصلی
│   │   ├── Program.cs                         # Entry point + DI
│   │   ├── appsettings.json                   # تنظیمات اصلی
│   │   ├── appsettings.Development.json       # تنظیمات محیط توسعه
│   │   ├── Dockerfile                         # Docker image
│   │   │
│   │   ├── Controllers/                       # لایه Controller
│   │   │   ├── TextToSqlController.cs         # /api/texttosql/{generate,schema,execute}
│   │   │   ├── SessionsController.cs          # /api/sessions CRUD
│   │   │   └── HealthController.cs            # /api/health
│   │   │
│   │   ├── Services/                          # لایه Business Logic
│   │   │   ├── IOllamaService.cs              # اینترفیس Ollama
│   │   │   ├── OllamaService.cs               # پیاده‌سازی اتصال به Ollama
│   │   │   ├── IVannaService.cs               # اینترفیس Vanna
│   │   │   ├── VannaService.cs                # پروکسی به میکروسرویس Python
│   │   │   ├── ISqlService.cs                 # اینترفیس SQL
│   │   │   ├── SqlService.cs                  # اجرای Read-Only + لیست سیاه
│   │   │   ├── ISessionService.cs             # اینترفیس Session
│   │   │   ├── SessionService.cs              # پیاده‌سازی Session
│   │   │   └── TextToSqlService.cs            # Orchestrator با Fallback
│   │   │
│   │   ├── Repositories/                      # لایه Data Access
│   │   │   ├── ISessionRepository.cs
│   │   │   └── SessionRepository.cs
│   │   │
│   │   ├── Models/
│   │   │   ├── Entities/                      # EF Core Entities
│   │   │   │   ├── Session.cs
│   │   │   │   └── SessionMessage.cs
│   │   │   ├── DTOs/                          # Data Transfer Objects
│   │   │   │   ├── SessionDtos.cs
│   │   │   │   ├── TextToSqlDtos.cs
│   │   │   │   └── CommonDtos.cs
│   │   │   └── Options/                       # Strongly-typed Options
│   │   │       ├── OllamaOptions.cs
│   │   │       ├── VannaOptions.cs
│   │   │       └── AppOptions.cs
│   │   │
│   │   ├── Data/                              # EF Core DbContext
│   │   │   └── SessionDbContext.cs
│   │   │
│   │   └── Middleware/                        # Cross-cutting concerns
│   │       └── GlobalExceptionMiddleware.cs
│   │
│   └── tests/TextToSqlApi.Tests/              # Unit Tests با xUnit
│       ├── TextToSqlApi.Tests.csproj
│       ├── Repositories/
│       │   └── SessionRepositoryTests.cs
│       ├── Services/
│       │   ├── SqlServiceValidationTests.cs
│       │   ├── OllamaServiceExtractTests.cs
│       │   ├── SessionServiceTests.cs
│       │   └── TextToSqlServiceTests.cs
│       └── Controllers/
│           └── SessionsControllerTests.cs
│
└── python-vanna-service/                      # میکروسرویس Python
    ├── main.py                                # FastAPI app
    ├── requirements.txt                       # وابستگی‌های Python
    ├── requirements-dev.txt                   # وابستگی‌های تست
    ├── Dockerfile                             # Docker image
    ├── .env.example                           # نمونه Environment Variables
    ├── .gitignore
    └── test_main.py                           # تست‌های pytest
```

---

## 🔧 عیب‌یابی

### مشکل ۱: Ollama در دسترس نیست

```
{"detail":"Ollama not available at http://localhost:11434"}
```

**راه‌حل‌ها**:
- اطمینان حاصل کنید Ollama در حال اجراست: `ollama serve`
- بررسی پورت: `curl http://localhost:11434/api/tags`
- مدل را دانلود کنید: `ollama pull qwen2.5-coder`

### مشکل ۲: خطای اتصال به SQL Server

```
Microsoft.Data.SqlClient.SqlException: Cannot connect to server
```

**راه‌حل‌ها**:
- Connection String را در `appsettings.json` بررسی کنید.
- اگر از Windows Auth استفاده می‌کنید، `Trusted_Connection=True` باشد.
- اگر از SQL Auth استفاده می‌کنید، `User Id` و `Password` را تنظیم کنید.
- در Docker، `Server=sqlserver` (نام سرویس) استفاده کنید.
- مطمئن شوید SQL Server Browser Service در حال اجراست (برای نام Instance).

### مشکل ۳: خطای EF Core Migration

```
Microsoft.Data.SqlClient.SqlException: Cannot open database "TextToSqlSessions"
```

**راه‌حل**:
- ابتدا اسکریپت `sql/01_create_session_tables.sql` را اجرا کنید.
- یا اگر می‌خواهید Migration خودکار اعمال شود، `App:ApplyMigrationsOnStartup=true` در `appsettings.json` باشد.

### مشکل ۴: Vanna در Docker بالا نمی‌آید

```bash
docker-compose logs vanna-service
```

معمولاً مشکل از نصب نبودن `msodbcsql18` است. Dockerfile آن را خودکار نصب می‌کند، اما اگر شبکه دچار مشکل باشد:
```bash
docker-compose build --no-cache vanna-service
```

### مشکل ۵: مدل SQL اشتباه تولید می‌کند

- شما دیتابیس را با جزئیات کامل ثبت کنید (DDL + نمونه داده).
- دمای مدل را کاهش دهید (`Ollama:Temperature=0.1` در `appsettings.json`).
- از مدل قوی‌تری مثل `qwen2.5-coder:7b` استفاده کنید.

### مشکل ۶: اجرای SQL با خطای Read-Only مواجه می‌شود

سیستم کلمات کلیدی زیر را بلاک می‌کند:
`DROP, DELETE, UPDATE, TRUNCATE, ALTER, INSERT, MERGE, GRANT, REVOKE, EXEC, EXECUTE, XP_CMDSHELL, SHUTDOWN, BACKUP, RESTORE, CREATE, ATTACH, DETACH`

اگر SQL شما فقط `SELECT` است اما بلاک می‌شود، احتمالاً نام ستونی با یکی از این کلمات هم‌نام است. در این صورت، نام ستون را در `[]` قرار دهید (مثلاً `[Update]`).

### مشکل ۷: تست‌ها fail می‌شوند

```bash
cd src/TextToSqlApi
dotnet test --logger "console;verbosity=detailed"
```

---

## 📞 پشتیبانی

در صورت بروز هرگونه مشکل، لاگ‌های سرویس را بررسی کنید:

```bash
# لاگ‌های .NET API
docker-compose logs -f texttosql-api

# لاگ‌های Vanna
docker-compose logs -f vanna-service

# لاگ‌های Ollama
docker-compose logs -f ollama
```

یا فایل‌های لاگ در `logs/texttosql-*.log` سرویس .NET را بررسی کنید.

---

## 📜 لایسنس

این پروژه به‌صورت آزاد ارائه می‌شود. استفاده تجاری و غیرتجاری مجاز است.

---

## 🎯 خلاصه Endpointها

| متد | مسیر | توضیح |
|---|---|---|
| `POST` | `/api/sessions` | ایجاد نشست جدید |
| `GET` | `/api/sessions/{id}` | دریافت تاریخچه نشست |
| `GET` | `/api/sessions?userName=x` | لیست نشست‌های کاربر |
| `DELETE` | `/api/sessions/{id}` | حذف نشست |
| `POST` | `/api/texttosql/generate` | تولید SQL از پرامپت |
| `POST` | `/api/texttosql/schema` | ثبت/به‌روزرسانی شما دیتابیس |
| `POST` | `/api/texttosql/execute` | اجرای SQL (فقط Read-Only) |
| `GET` | `/api/health` | بررسی سلامت همه کامپوننت‌ها |
| `GET` | `/health` | Health Check سبک ASP.NET Core |
| `GET` | `/swagger` | مستندات Swagger UI |

---

**موفق باشید! 🚀**
