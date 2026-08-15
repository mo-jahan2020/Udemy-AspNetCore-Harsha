using RoutingExample.CustomConstraints;

var builder = WebApplication.CreateBuilder(args);

// ۳. پیکربندی سرویس‌های مسیریابی (Routing) و اضافه کردن آن‌ها به سیستم تزریق وابستگی (DI Container)
// ۴. ثبت محدودیت مسیریابی سفارشی در "نقشه محدودیت‌ها" (ConstraintMap).
// - کلید "months": یک نام مستعار (Alias) است که ما خودمان انتخاب می‌کنیم تا در الگوهای URL از آن استفاده کنیم.
// - مقدار typeof(MonthsCustomConstraint): به سیستم می‌گوید هر جا در مسیر از "months" استفاده شد، 
//   منطق و کد داخل کلاس MonthsCustomConstraint را اجرا کن.
builder.Services.AddRouting(options =>
{
    options.ConstraintMap.Add("months", typeof(MonthsCustomConstraint));
});

var app = builder.Build();


// ۶. تعریف یک نقطه پایانی (Endpoint) با الگوی مسیر (Route Pattern) خاص.
// مثال URL معتبر: sales-report/2030/apr
// تحلیل بخش‌های متغیر در این الگو:
// الف) {year:int:min(1900)} -> استفاده از محدودیت‌های داخلی: مقدار باید عدد صحیح (int) و حداقل 1900 باشد.
// ب) {month:months} -> استفاده از محدودیت سفارشی ما! 
//    کلمه اول "month" نام پارامتر است. کلمه دوم "months" دقیقاً همان کلیدی است که در مرحله ۴ ثبت کردیم.

/*وقتی یک Constraint مقدار نادرست برمی‌گرداند، کد داخل بلوک
 * app.Map اصلاً اجرا نمی‌شود! سیستم مسیریابی آن مسیر را کلاً رد می‌کند و به دنبال مسیر بعدی می‌گردد.
 * اگر مسیر دیگری پیدا نکند،
 * خطای پیش‌فرض 404 (Not Found) را نشان می‌دهد.
*/

//Eg: sales-report/2030/apr
app.Map("sales-report/{year:int:min(1900)}/{month:months}", async context =>
{
    // ۷. استخراج مقدار year از دیکشنری مقادیر مسیر (RouteValues).
    // (چون محدودیت‌های int و min(1900) قبلاً تأیید شده‌اند، از معتبر بودن نوع داده مطمئن هستیم).
    int year = Convert.ToInt32(context.Request.RouteValues["year"]);
    // ۸. استخراج مقدار month از دیکشنری مقادیر مسیر.
    // (چون کلاس MonthsCustomConstraint قبلاً مقدار را بررسی و تأیید کرده، 
    // مطمئن هستیم که این متغیر فقط می‌تواند حاوی "apr", "jul", "oct" یا "jan" باشد).
    string? month = Convert.ToString(context.Request.RouteValues["month"]);
    // ۹. ارسال پاسخ نهایی به مرورگر کاربر

    await context.Response.WriteAsync($"sales report - {year} - {month}");
});
// ==========================================================
// ۲. مسیر جایگزین (Fallback) برای مدیریت خطای ماه نامعتبر
// ==========================================================
// نکته کلیدی: این مسیر دقیقاً همان الگوی URL را دارد، اما 
// محدودیت ":months" را ندارد.
// چون این مسیر "بعد" از مسیر بالا تعریف شده، سیستم مسیریابی 
// ابتدا مسیر بالا را چک می‌کند. اگر Constraint آن false برگرداند 
// (مثلاً کاربر "may" وارد کند)، سیستم به سراغ این مسیر می‌آید 
// و این بلوک اجرا می‌شود.
app.Map("sales-report/{year:int:min(1900)}/{month}", async context =>
{
    // تنظیم کد وضعیت HTTP به 400 (Bad Request) یک استاندارد حرفه‌ای 
    // برای زمانی است که ورودی کاربر نامعتبر است.
    context.Response.StatusCode = 400;

    // نمایش پیام سفارشی به کاربر در مرورگر
    await context.Response.WriteAsync("Sorry Month Is not Valid!");
});

app.Run();
