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
 * در مراحل طراحی این درست نیست که اطلاعات مهم مانند کلمه عبور ها در فایل کانفیگ ذخیره شوند
 * زیرا با ارسال آنها به گیت و یا ... این کلمات عبور مهم لو خواهد رفت 
 * راه حل ماکروسافت برای این مشکل ذخیره کلمات عبور و ... در رایانه شخص برنامه نویس با Secret Manager است
 * secret Manger این اطلاعات حطاتی را در جایی غیر از پروژه ذخیره میکند
 * دستورات آن در پاور شل خود ویژوال استودیو به این شرح است :
 * dotnet user-secrets init
 * dotnet user-secrets set "Key" "Value"
 * dotnet user-secrets list
 * برای دیدن محتویات ذخیره شده در user-secrets باید روی پروژه راست کلیک و گزینه manage user secrets را کیک کنید
 * با کیک روی آن خواهید دید که مسیر فایل باز شده غیر از مسیر پروژه است
 * دستور :dotnet user-secrets init در فایل جیسون خود پروژه خطی مانند زیر ایجاد میکند :
 * <UserSecretsId>551b0735-fe46-407a-9a59-581337074eee</UserSecretsId>
 * و فرآیند ذخیره اطلاعات وارد شده را در محلی بیرون از برنامه آغاز میکند
 * در ضمن فایل secrets.json را در مسیر خارج برنامه میسازد که نام فایل همان هشینگ تولید شده بالا میباشد
 * در مرحله دوم دستور زیر را میزنیم تا مقادیر در فایل بالا ذخیره شود :
 *  dotnet user-secrets set "weatherapi:ClientID" "ClientID From User Secrets commande"
 *  dotnet user-secrets set "weatherapi:ClientSecret" "ClientSecret From User Secrets commande"
 *  اگراکنون برنامه را اجرا کنید IOptions ابتدا از usersecret manager مقادیر را بارگذاری میکند.
 *  اگر چیزی در آنها پیدا نکرد به فایل appsettings.json مراجعه میکند
 *  
 */