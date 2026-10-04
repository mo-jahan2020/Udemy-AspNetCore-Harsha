var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

var app = builder.Build();
//IsDevelopment: محیط برنامه نویسی
//IsStaging: برنامه در سرورها نصب شده و کارشناسان کنترل کیفیت به آن دسترسی دارند
// IsEnvironment: برنامه در محیط حقیقی برای کاربر نهایی نصب شده است
if (app.Environment.IsDevelopment() || app.Environment.IsStaging() || app.Environment.IsEnvironment("Beta"))
{
  app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();
app.MapControllers();

app.Run();
