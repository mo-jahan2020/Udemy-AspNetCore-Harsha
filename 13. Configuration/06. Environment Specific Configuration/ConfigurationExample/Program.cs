using ConfigurationExample;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

//Supply an object of WeatherApiOptions (with 'weatherapi' section) as a service
// This allows you to inject IOptions<WeatherApiOptions> into your controllers
builder.Services.Configure<WeatherApiOptions>(builder.Configuration.GetSection("weatherapi"));

var app = builder.Build();

app.UseStaticFiles();

app.MapControllers();

app.Run();
