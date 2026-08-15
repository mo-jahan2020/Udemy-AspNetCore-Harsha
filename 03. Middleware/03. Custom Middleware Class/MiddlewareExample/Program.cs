
// Program.cs - نقطه‌ی شروع برنامه با Middleware سفارشی
// این فایل نحوه‌ی استفاده از سه نوع Middleware در ASP.NET Core رو نشون میده:
//   1. Middleware درون‌خطی (Inline) با app.Use و lambda
//   2. Middleware سفارشی با IMiddleware Interface
//   3. Middleware پایانی (Terminal) با app.Run
// using MiddlewareExample.CustomMiddleware;
// فضای نام مربوط به Middleware سفارشی که در فایل جداگانه تعریف شده
// برای استفاده از MyCustomMiddleware به این using نیاز داریم
// ایجاد WebApplicationBuilder (سازنده‌ی برنامه)
using MiddlewareExample.CustomMiddleware;
using MiddlewareExample.Midelwares;
var builder = WebApplication.CreateBuilder(args);
//  ثبت Middleware سفارشی در Container اصلی برنامه (Dependency Injection)
// AddTransient: یک نمونه‌ی جدید از MyCustomMiddleware برای هر درخواست ساخته میشه
// این ثبت فقط برای Middlewareهایی لازمه که اینترفیس IMiddleware رو پیاده‌سازی کنن
// چون UseMiddleware<T> وقتی T اینترفیس IMiddleware رو پیاده‌سازی کنه،
// نمونه رو از DI Container می‌گیره (نه با new ساختن مستقیم)
builder.Services.AddTransient<MyCustomMiddleware>();

//این باعث خطا میشود زیرا این میدلویر دارای سازنده است و نیاز به تزریق وابستگی در اینجا نیست و فقط آن را فراخوانی کنید
//builder.Services.AddTransient<NewMiddleware>();

// ساخت WebApplication از روی builder
// در این مرحله DI Container آماده میشه ولی Middleware Pipeline هنوز خالیه
var app = builder.Build();
// Middleware #1 - Middleware درون‌خطی (Inline) با app.Use
// app.Use: یک Middleware به Pipeline اضافه می‌کنه
// در اینجا به جای استفاده از کلاس جداگانه، مستقیم با lambda تعریف شده
// مزیت: سریع و ساده برای منطق‌های کوتاه
// عیب: برای منطق‌های پیچیده کد شلوغ میشه
app.Use(async (HttpContext context, RequestDelegate next) => {
        // نوشتن پیام در پاسخ (قبل از next - یعنی قبل از Middlewareهای بعدی)
    await context.Response.WriteAsync("From Midleware 1\n");
    // فراخوانی Middleware بعدی در زنجیره
    // اگه این خط نباشه، Pipeline اینجا متوقف میشه
    await next(context);
    // نکته: هیچ کدی بعد از next نیست، پس چیزی بر نمی‌گرده
    // برای نوشتن بعد از next، باید کد اینجا اضافه بشه
});
// Middleware #2 - Middleware سفارشی با IMiddleware Interface
// app.UseMiddleware<T>: یک Middleware کلاس‌محور به Pipeline اضافه می‌کنه
// T باید یکی از این دو حالت رو داشته باشه:
//   الف) اینترفیس IMiddleware رو پیاده‌سازی کنه ← نیاز به ثبت در DI داره
//   ب) کلاسی با سازنده‌ی RequestDelegate و متد Invoke باشه ← نیازی به ثبت در DI نیست
// در اینجا T = MyCustomMiddleware که IMiddleware رو پیاده‌سازی می‌کنه
// پس قبلش حتماً باید در DI ثبت می‌شد (که در بالا با AddTransient انجام شد)
app.UseMiddleware<MyCustomMiddleware>();


//چون سازنده دارد خودش تزریق وابستگی و ... را انجام داده و سپس اجرا میشود
app.UseMiddleware<NewMiddleware>();
//این هم میشود به خاطر تعریف انتهای میدل ویر 
//app.UseNewMiddleware();

// Middleware #3 - Terminal Middleware (پایان‌دهنده)
// app.Run: یک Terminal Middleware اضافه می‌کنه
// Terminal یعنی پایان Pipeline - این Middleware دیگه next رو صدا نمی‌زنه
// در اینجا HttpContext به صورت صریح (explicit) نوشته شده
app.Run(async (HttpContext context) => {
    // این خط آخرین چیزیه که در Pipeline اجرا میشه (بین Middlewareها)
    await context.Response.WriteAsync("From Middleware 3\n");
});
app.Run();