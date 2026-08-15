using System.Globalization;

var faCulture = new CultureInfo("fa-IR")
{
    DateTimeFormat = { Calendar = new GregorianCalendar() }
};
CultureInfo.DefaultThreadCurrentCulture = faCulture;
CultureInfo.DefaultThreadCurrentUICulture = faCulture;
// وگر نه تاریخ ها را به شمسی تبدیل میکند و کلی خطا رخ میدهد ما اینجا میگوییم تاریخ میلادی باشد

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
var app = builder.Build();

//app.UseStaticFiles();
app.MapControllers();

app.Run();
