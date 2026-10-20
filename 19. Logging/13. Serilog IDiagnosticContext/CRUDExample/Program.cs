using ServiceContracts;
using Services;
using Microsoft.EntityFrameworkCore;
using Entities;
using RepositoryContracts;
using Repositories;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

//Serilog باید پکیج Serilog.Extemtions.Hosting در لایه Services نصب شود
// حال در سرویس 
/*
 * IDiagnosticContext یک اینترفیس در پکیج Serilog.AspNetCore است که به شما امکان می‌دهد بدون نیاز به
 * ILogger، پراپرتی‌های دلخواه (context) به رویدادهای لاگ اضافه کنید.
 * وقتی UseSerilog() را روی host صدا می‌زنید، این اینترفیس به‌صورت خودکار با 
 * lifetime از نوع Scoped در سیستم DI ثبت می‌شود.
 * Set(name, value): اضافه/به‌روزرسانی یک پراپرتی روی همه لاگ‌های همان Scope
 * Get(name): خواندن مقدار یک پراپرتی ثبت‌شده
 * Remove(name): حذف یک پراپرتی از Context
 * SetCorrelationId:متد کمکی برای ثبت شناسه همبستگی (در نسخه‌های جدید)
 */
/*
 * نکات مهم
حتماً باید .Enrich.FromLogContext() در تنظیمات Logger فعال باشد، وگرنه پراپرتی‌ها در خروجی ظاهر نمی‌شوند.
اگر UseSerilog() را صدا نزنید، IDiagnosticContext در DI ثبت نمی‌شود و باید آن را دستی رجیستر کنید: services.AddScoped<IDiagnosticContext, DiagnosticContext>().
به دلیل Scoped بودن، آن را در سرویس‌های Singleton مستقیم تزریق نکنید (مگر با ایجاد Scope جدید).
 * 
 */
builder.Host.UseSerilog((HostBuilderContext context, IServiceProvider services, LoggerConfiguration loggerConfiguration) => {

 loggerConfiguration
 .ReadFrom.Configuration(context.Configuration) //read configuration settings from built-in IConfiguration
 .ReadFrom.Services(services); //read out current app's services and make them available to serilog
} );


builder.Services.AddControllersWithViews();

//add services into IoC container
builder.Services.AddScoped<ICountriesRepository, CountriesRepository>();
builder.Services.AddScoped<IPersonsRepository, PersonsRepository>();

builder.Services.AddScoped<ICountriesService, CountriesService>();
builder.Services.AddScoped<IPersonsService, PersonsService>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
 options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});


builder.Services.AddHttpLogging(options =>
{
 options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestProperties | Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponsePropertiesAndHeaders;
});

var app = builder.Build();

app.UseSerilogRequestLogging();

//create application pipeline
if (builder.Environment.IsDevelopment())
{
 app.UseDeveloperExceptionPage();
}

app.UseHttpLogging();

//app.Logger.LogDebug("debug-message");
//app.Logger.LogInformation("information-message");
//app.Logger.LogWarning("warning-message");
//app.Logger.LogError("error-message");
//app.Logger.LogCritical("critical-message");

if (builder.Environment.IsEnvironment("Test") == false)
 Rotativa.AspNetCore.RotativaConfiguration.Setup("wwwroot", wkhtmltopdfRelativePath: "Rotativa");

app.UseStaticFiles();
app.MapControllers();

app.Run();

public partial class Program { } //make the auto-generated Program accessible programmatically

