var builder = WebApplication.CreateBuilder(args);

// initialize MVC services
builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseStaticFiles();

//map default controller route
// HomeController location: Controllers/HomeController.cs
app.MapControllers();

//دیگر به این نیاز نیست
//app.MapGet("/", () => "Hello World!");  
app.Run();
