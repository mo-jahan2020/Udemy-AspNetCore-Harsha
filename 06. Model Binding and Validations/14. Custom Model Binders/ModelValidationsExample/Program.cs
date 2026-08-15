using System.Globalization;

var faCulture = new CultureInfo("fa-IR")
{
    DateTimeFormat = { Calendar = new GregorianCalendar() }
};
CultureInfo.DefaultThreadCurrentCulture = faCulture;
CultureInfo.DefaultThreadCurrentUICulture = faCulture;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
var app = builder.Build();

//app.UseStaticFiles();
app.MapControllers();

app.Run();
