using ModelValidationsExample.CustomModelBinders;

var builder = WebApplication.CreateBuilder(args);
// افزودن سرویس مدل بایندر
// مهم اینجاست که باید حتما پیش فرض از این کلاس استفاده کنیم وگرنه کنترلر از بایند غیر کاستوم استفاده خواهد کرد استفاده کنیم
// اندیس 0 : بایندر پیش فرض بشود اگر عدد دیگر بدهیم پیش فرض نخواهد شد و باز قبلی لود خوهد شد
builder.Services.AddControllers(options => 
{
 //
 options.ModelBinderProviders.Insert(0, new PersonBinderProvider());
});
//نکته مه اینجاست این کار در برنامه های ساده کاربرد ندارد  بلکه در برنامه های پیچیده با کلاس ها و سرویس های نا مشکل کاربرد ددارد

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();

app.Run();
