var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

var app = builder.Build();
// مجیط در فایل زیر تنظیم میشود:
// Properties\launchSettings.json
if (app.Environment.IsDevelopment())
{
    // با این خط دیگر خطای کلی 500 صادر نمیشود بلکه خطا با جزئیات به کاربر نشان داده خواهد شد
  app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();
app.MapControllers();

app.Run();
