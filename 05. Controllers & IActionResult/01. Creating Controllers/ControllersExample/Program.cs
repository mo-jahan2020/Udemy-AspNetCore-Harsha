var builder = WebApplication.CreateBuilder(args);
//--------------------------------------------------------------------------
//این تعریف و فراخوانی پایین آن برای برنامه های API مناسب است
builder.Services.AddControllers(); //adds all the controller classes as services
//app.MapControllers();//for attribute routing

//--------------------------------------------------------------------------
// For Views | MVC | Razor Pages:
//علاوه بر controllerهای MVC، امکانات مربوط به view rendering را هم اضافه می‌کند
//builder.Services.AddControllersWithViews();
// دیگر app.MapControllersWithViews() نداریم؟
// بجای آن باید از یکی از این دو استفاده کرد:

//1- مسیر را در Program.cs به‌صورت عمومی تعریف می‌کنی
//app.MapControllerRoute(name: "default",pattern: "{controller=Home}/{action=Index}/{id?}");
//app.MapControllerRoute()// for conventional routing

//2- Attribute routing: مسیر را هر action جداگانه مشخص می‌کند.
//[Route("api/products")]
//public class ProductsController : ControllerBase
//app.MapControllers()//for attribute routing

//--------------------------------------------------------------------------
//For Views | MVC | Razor Pages
//فقط سرویس‌های مسیریابی را ثبت می‌کند
//builder.Services.AddRouting();
//app.UseRouting();
//--------------------------------------------------------------------------

var app = builder.Build();

//For API App
app.MapControllers();//for attribute routing


app.Run();
