using CitiesManager.WebAPI.DatabaseContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// چون این فیلتر ها بصورت سرتسری تعریف شده اند لذا روی تمام کنترلر ها و اکشن ها اعمال خواهد شد
builder.Services.AddControllers(options => {
 options.Filters.Add(new ProducesAttribute("application/json"));// این یک فیلتر برای مقادیر ورودی است که آن را منحصر به جیسون خواهد کرد
 options.Filters.Add(new ConsumesAttribute("application/json"));// این یک فیلتر برای مقادیر خروجی است که آن را منحصر به جیسون خواهد کرد
})
 .AddXmlSerializerFormatters();// این خط برای پشتیبانی از XML در برنامه اجباری است
// برای جیسون نیاز به هیچ خطی نیست زیرا برنامه پیش فرض از جیسون پشتیبانی میکند

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
 options.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
});

//Swagger
builder.Services.AddEndpointsApiExplorer(); //Generates description for all endpoints
builder.Services.AddSwaggerGen(options => {
 options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "api.xml"));

}); //generates OpenAPI specification


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHsts();
app.UseHttpsRedirection();

app.UseSwagger(); //creates endpoint for swagger.json
app.UseSwaggerUI(); //creates swagger UI for testing all Web API endpoints / action methods

app.UseAuthorization();

app.MapControllers();

app.Run();
