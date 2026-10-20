using ServiceContracts;
using Services;
using Microsoft.EntityFrameworkCore;
using Entities;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

//add services into IoC container
builder.Services.AddScoped<ICountriesService, CountriesService>();
builder.Services.AddScoped<IPersonsService, PersonsService>();

builder.Services.AddDbContext<PersonsDbContext>(options => {
  options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// هر تغییری در لایه دیتا Entities و زدن دستور ذیل همه تغییرات در Migration اعمال میشود
// ولی نکته اینجاست بیزینس خود برنامه با شماست 
// ونیز حواستان به تغییرات دیگر باشد مثلاً با تغییر یک فیلد باید بروزرسانی  StoredP را هم با یک Migration دیگر تغییر دهید

var app = builder.Build();

if (builder.Environment.IsDevelopment())
{
  app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();
app.MapControllers();

app.Run();
