using CitiesManager.WebAPI.DatabaseContext;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
 options.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
});
// نقش هر کدام از پکیج های موجود برای Sqagger:
/*
 * 1- OpenAPI : یک استاندارد/مشخصات (Specification) — سند JSON یا YAML
 *  OpenAPI:  مثل فرمت PDF یک استانداردِ سند است.
 *      «قرارداد مکتوب» API شما:
 *      - چه مسیرهایی وجود دارد،
 *      - چه پارامترهایی می‌گیرند،
 *      - چه چیزی برمی‌گردانند،
 *      - خطاها چیست
 * 2- Swashbuckle.AspNetCore.Swagger : مجموعه ابزارها (نه استاندارد) — عمدتاً Swagger UI 
 *  Swagger UI:  مثل Adobe Reader ابزاری که سند را می‌خواند و نشان می‌دهد.
 *      رابط کاربری تعاملی که سند OpenAPI را می‌خواند و زیبا رندر می‌کند به همراه امکان تست زنده
 *      
 * 3- Swashbuckle.AspNetCore : یک پکیج NuGet مخصوص ASP.NET Core 
 *  Swashbuckle :  مثل پرینتر داخل برنامه‌ی شماست از روی کد C# شما، سند PDF یعنی swagger.json تولید می‌کند — و به‌علاوه یک Reader هم همراهش embed کرده.
 *      مترجم داخل پروژه‌ی شما که از روی کنترلرهای C#، سند OpenAPI می‌سازد و Swagger UI را هم Hosting می‌کند
 */


/*
 * و اما Swashbuckle خودش از ۳ زیرپکیج تشکیل شده  :
 * Swashbuckle.AspNetCore.SwaggerGen : تولید سند OpenAPI از روی کنترلرها/مدل‌ها/Route ها : AddSwaggerGen() در این فایل
 * Swashbuckle.AspNetCore.Swagger : سرو کردن سند JSON در یک URL : UseSwagger() در این فایل
 * Swashbuckle.AspNetCore.SwaggerUI : سرو کردن رابط تعاملی HTML/JS (که embed شده در DLL پکیج) : UseSwaggerUI() در این فایل
 * 

 */
//Swagger مهم nswag هم همین کار را انجام می دهد

/*
 * ┌── مرحله ۱: ثبت سرویس‌ها در DI (کارخانه را آماده می‌کنند) ──┐
│  builder.Services.AddEndpointsApiExplorer();   ← منبع داده │
│  builder.Services.AddSwaggerGen();              ← مولد سند │
└────────────────────────────────────────────────────────────┘
                          ↓ خروجی‌شان: یک SwaggerDocument آماده
┌── مرحله ۲: Middleware ها (نتیجه را در Pipeline سرو می‌کنند) ─┐
│  app.UseSwagger();      ← سند JSON را در URL می‌گذارد       │
│  app.UseSwaggerUI();    ← صفحه‌ی تعاملی را در URL می‌گذارد   │
└─────────────────────────────────────────────────────────────┘
 */
//این خط آن کاتالوگ را در DI ثبت و پر می‌کند. چرا مهم است؟ چون Swashbuckle خودش نمی‌داند پروژه شما چه API هایی دارد — بلکه از همین ApiExplorer «سوال می‌پرسد»
builder.Services.AddEndpointsApiExplorer(); //Generates description for all endpoints
/*
 * مولد سند را در DI ثبت می‌کند. در Runtime، این مولد:

متادیتا را از ApiExplorer می‌خواند
با Reflection مدل‌ها (مثل کلاس City پروژه شما) را به Schema های OpenAPI تبدیل می‌کند
یک آبجکت SwaggerDocument می‌سازد (همان استاندارد OpenAPI به شکل JSON)
همه‌ی تنظیمات سفارشی‌سازی هم اینجا انجام می‌شود — عنوان و نسخه، کامنت‌های XML، احراز هویت JWT و... 
 */
builder.Services.AddSwaggerGen(); //generates OpenAPI specification


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHsts();
app.UseHttpsRedirection();

/*
 * یک Middleware که می‌گوید: هرکس به مسیر /swagger/v1/swagger.json آمد، سند تولیدشده را به‌صورت JSON تحویل بده.
 * یعنی سند OpenAPI را در یک آدرس قابل دانلود منتشر می‌کند
 */
app.UseSwagger(); //creates endpoint for swagger.json

/* Middleware ای که رابط Swagger UI را سرو می‌کند —
 * همان صفحه‌ی معروف HTML/JS تعاملی که در مسیر /swagger باز می‌شود (داخل خود پکیج embed شده و نیازی به فایل اضافه ندارد).
 * این UI سند JSON مرحله‌ی قبل را می‌خواند و رندر می‌کند:
*/
app.UseSwaggerUI(); //creates swagger UI for testing all Web API endpoints / action methods

app.UseAuthorization();

app.MapControllers();

app.Run();
