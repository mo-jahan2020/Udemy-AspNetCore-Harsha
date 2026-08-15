var builder = WebApplication.CreateBuilder(args);
//در درس قبلی مفصل به آن پرداختیم
builder.Services.AddControllers().AddXmlSerializerFormatters();
var app = builder.Build();

app.UseStaticFiles();
app.MapControllers();

app.Run();
