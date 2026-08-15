var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
// برای قبول کردن XML اجباری است
// در ضمن بهتر است در postman-> Headers-> Content-Type=application/xml
// ولی برای ارسال مقادیر جیسون نیاز به این کارها نیست زیرا پیش فرض برنامه و پستمن جیسون است

builder.Services.AddControllers().AddXmlSerializerFormatters();
var app = builder.Build();

//app.UseStaticFiles();
app.MapControllers();

app.Run();
