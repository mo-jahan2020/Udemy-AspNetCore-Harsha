using ServiceContracts;
using Services;
using Microsoft.EntityFrameworkCore;
using Entities;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

//add services into IoC container
builder.Services.AddScoped<ICountriesService, CountriesService>();
builder.Services.AddScoped<IPersonsService, PersonsService>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
 options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

//Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=PersonsDatabase;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False

var app = builder.Build();

if (builder.Environment.IsDevelopment())
{
 app.UseDeveloperExceptionPage();
}

Rotativa.AspNetCore.RotativaConfiguration.Setup("wwwroot", wkhtmltopdfRelativePath: "Rotativa");

app.UseStaticFiles();
app.MapControllers();

app.Run();
/*
 * تا اینجا لایه سرویس ما مستقیم با لایه دیتا یا ApplicationDbContext در ارتباط است و این باعث می‌شود
 * که اگر بخواهیم لایه دیتا را تغییر دهیم، لایه سرویس هم تغییر کند.
 * برای حل این مشکل، ما یک لایه Repository ایجاد می‌کنیم که بین لایه سرویس و لایه دیتا قرار می‌گیرد.
 * این لایه Repository مسئولیت دسترسی به داده‌ها را بر عهده دارد و سرویس‌ها از طریق این لایه با داده‌ها تعامل می‌کنند.
 * به این ترتیب، اگر بخواهیم لایه دیتا را تغییر دهیم، فقط نیاز است که لایه Repository را تغییر دهیم 
 * و لایه سرویس بدون تغییر باقی می‌ماند.
 */
