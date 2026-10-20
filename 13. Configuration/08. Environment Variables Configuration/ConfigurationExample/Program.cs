using ConfigurationExample;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

//Supply an object of WeatherApiOptions (with 'weatherapi' section) as a service
builder.Services.Configure<WeatherApiOptions>(builder.Configuration.GetSection("weatherapi"));

var app = builder.Build();

app.UseStaticFiles();

app.MapControllers();

app.Run();
/*
 * دات‌نت به‌صورت خودکار تنظیمات را از چند منبع می‌خواند و به ترتیب اولویت روی هم می‌گذارد:
 * appsettings.json                  ← اولویت پایین
 * appsettings.{Environment}.json
 * Environment Variables             ← اولویت بالا (مقادیر قبلی را بازنویسی می‌کند)
 * Command line arguments
 * در نام Environment Variable ها در ویندوز مجاز نیست! برای همین در محیط‌های مختلف از __ استفاده می‌شود
 * دستور پاور شل ذخیره و نگهدری مقدیر به این شرح است :
 * $Env:weatherapi__ClientID = "override-ID-from-env-456"
 * $Env:weatherapi__ClientSecret = "override-Secret-from-env-456"
 * مهم این دستورات تا با ز بودن شل پاور شل اعتبار دارد و با بسته شدن شل این مقادیر از بین میروند
 * برای دائمی کردن این مقادیر باید از setx  استفاده کنید:
 * setx weatherapi__ClientID "real-secret-999"
 * برای دیدن اثز آن باید یک بار پنجره را ببندید و سپس باز کنید و حال با زدن دستور ذیل آن مقدار دیده خواهد شد:
 * $Env:weatherapi__ClientID
 * وقتی به‌طور عادی می‌نویسید dotnet run، ویژوال استودیو/دات‌نت فایل Properties/launchSettings.json را می‌خواند.
 * این فایل ممکن است خودش Environment Variable هایی تعریف کرده باشد که مقادیر شما را خنثی یا گمراه‌کننده کنند.
 * لذا باید برای دیدن نتیجه اسن دستورات بالا برنامه را به این شکل اجرا کنیم:
 * dotnet run --no-launch-profile
 * ولی چرا ؟
 * چون در دنیای واقعی (Production) دقیقاً همین‌طور کار می‌کنند:
 * Docker: با فلگ -e مقادیر را تزریق می‌کنند
 * Azure App Service: بخش App Settings همان Environment Variable ها هستند
 * CI/CD و Kubernetes: تنظیمات حساس از این طریق داده می‌شود
 * منطقش هم ساده است: شما نمی‌خواهید پسوردها و API Key ها را در appsettings.json بگذارید که وارد Git می‌شود!
 * به‌جای آن مقدار پیش‌فرض بی‌ضرر را در JSON می‌گذارید و مقادیر واقعی را در سرور به‌صورت Environment Variable تزریق می‌کنید —
 * بدون اینکه نیاز به تغییر کد یا فایلی باشد.
 */
