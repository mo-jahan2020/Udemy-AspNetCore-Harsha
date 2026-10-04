using ModelValidationsExample.CustomModelBinders;
using System.Globalization;

var faCulture = new CultureInfo("fa-IR")
{
    DateTimeFormat = { Calendar = new GregorianCalendar() }
};
CultureInfo.DefaultThreadCurrentCulture = faCulture;
CultureInfo.DefaultThreadCurrentUICulture = faCulture;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers(options => {
  //options.ModelBinderProviders.Insert(0, new PersonBinderProvider());
});

builder.Services.AddControllers().AddXmlSerializerFormatters();
var app = builder.Build();

app.UseStaticFiles();
app.MapControllers();

app.Run();
