//// این namespace کلاس StringValues را فراهم می‌کند.
//// StringValues نوعی است که می‌تواند صفر، یک یا چند مقدار رشته‌ای را نگه دارد
//// (چون یک کلید در query می‌تواند چند مقدار داشته باشد، مثلاً id=1&id=2).
//using Microsoft.Extensions.Primitives;

//// این namespace کلاس StreamReader را برای خواندن جریان (Stream) داده‌ها فراهم می‌کند.
//using System.IO;

//// یک "سازنده‌ی برنامه" می‌سازیم. builder امکانات پیکربندی، سرویس‌ها (DI) و ... را آماده می‌کند.
//// args همان آرگومان‌های خط فرمان است که هنگام اجرای برنامه داده می‌شود.
//var builder = WebApplication.CreateBuilder(args);

//// با build کردن builder، شیء app ساخته می‌شود.
//// app همان "اپلیکیشن وب" است که درخواست‌های HTTP را دریافت و پاسخ می‌دهد.
//var app = builder.Build();

//// app.Run در اینجا یک "middleware انتهایی (terminal)" ثبت می‌کند.
//// یعنی برای هر درخواست ورودی، این تابع اجرا می‌شود و دیگر درخواست به جای دیگری منتقل نمی‌شود.
//// context تمام اطلاعات درخواست (Request) و پاسخ (Response) را در خود دارد.

//app.Run(async (HttpContext context) =>
//{
//    // Request.Form هم form-data و هم x-www-form-urlencoded را می‌فهمد
//    if (context.Request.HasFormContentType)
//    {
//        IFormCollection form = await context.Request.ReadFormAsync();

//        if (form.ContainsKey("firstName"))
//        {
//            string? firstName = form["firstName"][0];
//            await context.Response.WriteAsync(firstName);
//        }
//    }
//});

//// اجرای برنامه؛ از این‌جا به بعد اپلیکیشن شروع به گوش‌دادن (listen) روی پورت می‌کند
//// و منتظر دریافت درخواست‌ها می‌ماند. این خط برنامه را زنده نگه می‌دارد.
//app.Run();