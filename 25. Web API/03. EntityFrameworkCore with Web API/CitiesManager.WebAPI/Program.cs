using CitiesManager.WebAPI.DatabaseContext;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
 options.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
// ابتدا پکیج زیر را نصب کنید تا خطا رفع شود
// System.Security.Cryptography.Pkcs
// در نهایت در package Manager:update-database
//app.UseHsts();
//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
