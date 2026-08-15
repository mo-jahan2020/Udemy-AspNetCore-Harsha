using System.Net;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// ۱. اول مسیریاب را روشن می‌کنیم تا آدرس‌ها را اسکن کند
// البته در این برنامه کوچک اینکار بصورت اتوماتیک انجام میشوند
// . نیاز به نوشتن آن نیست 
// اگر در برنامه Autorazation , Authertication
// نوشتی و یا یک میان افزار خودت داشتی حتماً این کد باید نوشته شود 
// دلیل آن چیست : زیرا اول کاربر به صفحه آدرس دهی شود تا دو سیستم فوق بفهمند به آن صفحه دسترسی دارد یا نه؟ 
//app.UseRouting();

/*
  UseRouting: "بگو ببینم این آدررس مربوط به کدام تابع می‌شود؟" فقط تحقیق می‌کند
  UseEndpoints: "حالا که فهمیدم مربوط به کدام تابع است، بروم آن تابع را اجرا کنم." اقدام می‌کند
*/
//Routing is automatically enabled.
//No need for app.UseRouting() anymore

//Endpoints are defined directly on the "app" object.
//We will add endpoints here in the next lecture.

//app.MapGet("/", () => "hello ali11");
// ۳. در نهایت می‌گوییم اگر مسیریاب آدرس زیر را پیدا کرد، این کد را اجرا کن


//البته این متد از تعریف قدیمی است  بخاطر همین هشدار دارد
app.UseEndpoints(endpoints =>
{
    endpoints.MapGet("/", async (context) =>
    {
        await context.Response.WriteAsync("homa page hello ali11");
    });
    endpoints.MapGet("/hello", async (context) =>
    {
        await context.Response.WriteAsync("/hello Hello World!this code run with Endpoints.");
    });
});
// بجای بالا از این معادل استفاده کنید:
// بدون نیاز به UseEndpoints! مستقیم روی app تعریف کن
/*
app.MapGet("/", async (context) =>
{
    await context.Response.WriteAsync("homa page hello ali11");
});

app.MapGet("/hello", async (context) =>
{
    await context.Response.WriteAsync("/hello Hello World!this code run with Endpoints.");
});
*/
app.Run(async context =>
{
    await context.Response.WriteAsync("run app");
});
app.Run();
