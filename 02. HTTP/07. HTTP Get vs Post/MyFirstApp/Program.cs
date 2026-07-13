// این namespace کلاس StringValues را فراهم می‌کند.
// StringValues نوعی است که می‌تواند صفر، یک یا چند مقدار رشته‌ای را نگه دارد
// (چون یک کلید در query می‌تواند چند مقدار داشته باشد، مثلاً id=1&id=2).
using Microsoft.Extensions.Primitives;

// این namespace کلاس StreamReader را برای خواندن جریان (Stream) داده‌ها فراهم می‌کند.
using System.IO;

// یک "سازنده‌ی برنامه" می‌سازیم. builder امکانات پیکربندی، سرویس‌ها (DI) و ... را آماده می‌کند.
// args همان آرگومان‌های خط فرمان است که هنگام اجرای برنامه داده می‌شود.
var builder = WebApplication.CreateBuilder(args);

// با build کردن builder، شیء app ساخته می‌شود.
// app همان "اپلیکیشن وب" است که درخواست‌های HTTP را دریافت و پاسخ می‌دهد.
var app = builder.Build();

// app.Run در اینجا یک "middleware انتهایی (terminal)" ثبت می‌کند.
// یعنی برای هر درخواست ورودی، این تابع اجرا می‌شود و دیگر درخواست به جای دیگری منتقل نمی‌شود.
// context تمام اطلاعات درخواست (Request) و پاسخ (Response) را در خود دارد.
app.Run(async (HttpContext context) =>
{

    // بدنه‌ی درخواست (Request Body) یک جریان (Stream) از بایت‌هاست.
    // با StreamReader آن را برای خواندن به‌صورت متن آماده می‌کنیم.
    StreamReader reader = new StreamReader(context.Request.Body);

    // کل محتوای بدنه‌ی درخواست را به‌صورت غیرهمزمان (async) می‌خوانیم و در یک رشته می‌ریزیم.
    // await یعنی منتظر می‌مانیم خواندن تمام شود، بدون اینکه ترد (thread) بلاک شود.
    string body = await reader.ReadToEndAsync();

    // بدنه معمولاً به‌شکل "firstName=Ali&lastName=Reza" است (فرمت form-urlencoded).
    // ParseQuery این رشته را تجزیه می‌کند و به یک دیکشنری از کلید/مقدار تبدیل می‌کند.
    //با postman باید نوع دیتا را x-www-form بفرستید
     Dictionary<string, StringValues> queryDict = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(body);

    // بررسی می‌کنیم که آیا کلیدی به نام "firstName" در داده‌های ارسالی وجود دارد یا نه.
    if (queryDict.ContainsKey("firstName"))
    {
        // چون هر کلید می‌تواند چند مقدار داشته باشد، با [0] اولین مقدار را می‌گیریم.
        // نوع string? یعنی این مقدار ممکن است null باشد.
        string? firstName = queryDict["firstName"][0];

        // مقدار firstName را به‌عنوان پاسخ به مرورگر/کلاینت می‌نویسیم.
        await context.Response.WriteAsync(firstName);
    }
});

// اجرای برنامه؛ از این‌جا به بعد اپلیکیشن شروع به گوش‌دادن (listen) روی پورت می‌کند
// و منتظر دریافت درخواست‌ها می‌ماند. این خط برنامه را زنده نگه می‌دارد.
app.Run();