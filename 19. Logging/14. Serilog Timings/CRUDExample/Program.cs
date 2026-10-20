using ServiceContracts;
using Services;
using Microsoft.EntityFrameworkCore;
using Entities;
using RepositoryContracts;
using Repositories;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

//Serilog پکیج زیر نصب شود در servives: Serilog.Timings
// این لاگ زمان شروع و پایان یک متد یا یک دستور را استخراج کرده و ثبت میکند
//هر عملیات (کوئری دیتابیس، صدا زدن API خارجی، خواندن از کش، تولید گزارش و...) دقیقاً یک رویداد لاگ ساخت‌یافته تولید می‌کند که در پایان کار صادر می‌شود و شامل مدت زمان اجرا است.
/*
 * رفتارها و قابلیت‌های کلیدی:
 * Complete(): اگر بلوک بدون Complete تمام شود (مثلاً Exception پرتاب شود)، عملیات شکست‌خورده تلقی و رویدادی با Outcome = "abandoned" در سطح Warning ثبت می‌شود — حتی Elapsed هم ثبت می‌شود، پس می‌فهمید کالاهای شکست‌خورده چقدر کند بوده‌اند!
 * Operation.At(level):تغییر سطح ثبت: Operation.At(LogEventLevel.Debug).Time("Preparing zip archive") — پارامتر دوم هم سطح abandonment را تعیین می‌کند
 * Operation.Time(...):نسخه‌ی خودکار: با Dispose شدن، خودبه‌خود «completed» ثبت می‌شود (بدون نیاز به صدا زدن Complete)
 * op.Cancel():وقتی عملیات اصلاً موضوعیت نداشت (مثلاً کش Hit شد و نیازی به ادامه نبود)، رویداد را کاملاً حذف می‌کند تا لاگ شلوغ نشود 
 * OperationId:به‌جای متد استاتیک: logger.BeginOperation(...) و logger.TimeOperation(...)
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

