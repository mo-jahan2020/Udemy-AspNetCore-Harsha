// ایجاد سازنده و ساخت برنامه (همان مراحل استاندارد قبلی)
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// تعریف میدل‌ویر پردازش‌کننده درخواست‌ها
app.Run(async (HttpContext context) =>
{
    // تنظیم نوع خروجی به HTML تا مرورگر تگ‌ها را تفسیر کند
    context.Response.Headers["Content-type"] = "text/html";

    // بررسی اینکه آیا متد درخواست، از نوع GET است یا خیر؟
    // (ما معمولاً برای دریافت اطلاعات از متد GET استفاده می‌کنیم)
    //http://localhost:6734/?id=123
    if (context.Request.Method == "GET")
    {
        // بررسی وجود کلیدِ "id" در کوئری استرینگ (Query String) آدرس درخواست
        // کوئری استرینگ قسمتی از URL است که بعد از علامت سوال (?) می‌آید. مثلاً: ?id=12
        if (context.Request.Query.ContainsKey("id"))
        {
            // استخراج مقداری که کاربر برای "id" ارسال کرده است.
            // مثلاً اگر آدرس ?id=ali باشد، متغیر id برابر "ali" می‌شود.
            string id = context.Request.Query["id"].ToString();

            // چاپ مقدار استخراج شده در صفحه مرورگر
            await context.Response.WriteAsync($"<p>id={id}</p>");
        }
        // نکته: اگر کاربر آدرسی وارد کند که شامل "id" نباشد (مثلاً فقط مسیر اصلی)، 
        // این بلوک کد هیچ خروجی‌ای تولید نمی‌کند و صفحه خالی می‌ماند.
    }
});

// اجرای برنامه و شروع گوش دادن به درخواست‌ها
app.Run();