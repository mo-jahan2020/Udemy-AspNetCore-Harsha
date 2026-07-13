// ایجاد یک WebApplication Builder با تنظیمات پیش‌فرض
// args آرگومان‌های خط فرمان را دریافت می‌کند (مثل تنظیم پورت از طریق command line)
var builder = WebApplication.CreateBuilder(args);

// ساخت نمونه نهایی برنامه بر اساس تنظیمات Builder
var app = builder.Build();

// app.Run با یک Delegate: این روش یک Middleware سطح پایین ثبت می‌کند
// که تمامی درخواست‌های ورودی (بدون توجه به مسیر یا متد) را پردازش می‌کند.
// توجه: در این حالت شما به صورت دستی باید متد، مسیر و هدرها را بررسی کنید.
app.Run(async (HttpContext context) =>
{
    // تنظیم هدر پاسخ برای اینکه مرورگر یا کلاینت بداند محتوای ارسالی HTML است
    // بدون این تنظیم، مرورگر ممکن است محتوا را به صورت متن خام نمایش دهد
    context.Response.Headers["Content-type"] = "text/html";

    // بررسی وجود هدر "User-Agent" در درخواست
    // هدرها (Headers) اطلاعاتی هستند که کلاینت (مرورگر، Postman، curl و...) درباره خودش
    // یا درخواستش به سرور ارسال می‌کند (برخلاف Query String که در URL قرار می‌گیرد)
    if (context.Request.Headers.ContainsKey("User-Agent"))
    {
        // خواندن مقدار هدر User-Agent
        // نوع برگشتی StringValues است (چون یک هدر می‌تواند چند مقدار داشته باشد)
        // با ToString() آن را به string تبدیل می‌کنیم تا از Warning مربوط به Nullable جلوگیری شود
        string userAgent = context.Request.Headers["User-Agent"].ToString();

        // نوشتن مقدار User-Agent در بدنه پاسخ به صورت HTML
        await context.Response.WriteAsync($"<p>User-Agent: {userAgent}</p>");
    }
    else
    {
        // در صورتی که کلاینت هدر User-Agent را ارسال نکرده باشد
        await context.Response.WriteAsync("<p>هدر User-Agent ارسال نشده است.</p>");
    }
});

// شروع سرور و گوش دادن به درخواست‌های ورودی
// این متد تا زمانی که برنامه متوقف شود، اجرا را مسدود (Block) می‌کند
app.Run();

/*
  # در ویندوز (CMD یا PowerShell)
curl http://localhost:6734/ -H "User-Agent: MyCustomBot/1.0"

# در لینوکس یا مک
curl http://localhost:6734/ -H "User-Agent: MyCustomBot/1.0"
البته با فراخوانی در مرورگر با هر آدرسی خود مرورگر User-Agent رارپیش فرض مقدار دهی میکند.
پس حتماً خروجی زیر را خواهید دید:
browser: http://localhost:6734/
User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:152.0) Gecko/20100101 Firefox/152.0
 */