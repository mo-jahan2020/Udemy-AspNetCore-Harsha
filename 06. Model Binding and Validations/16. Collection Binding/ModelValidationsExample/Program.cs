using ModelValidationsExample.CustomModelBinders;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers(options => 
{
    // چون میخواهیم از کلاس اصلی پرسون تغییر ایجاد کنیم این خط را غیر فعال میکنیم تا از آن استفاده کند نه این
 //options.ModelBinderProviders.Insert(0, new PersonBinderProvider());
});


var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();

app.Run();
