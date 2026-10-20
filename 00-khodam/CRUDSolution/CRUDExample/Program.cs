// step-0 : dotnet tool install --global dotnet-ef
using Entities;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
//add services into IoC container
builder.Services.AddScoped<ICountriesService, CountriesService>();
builder.Services.AddScoped<IPersonsService, PersonsService>();
// step-4
//OR builder.Configuration.GetConnectionString("DefaultConnection")
builder.Services.AddDbContext<PersonsDbContext>(options => 
{
    options.UseSqlServer(builder.Configuration["ConnectionStrings:DefaultConnection"]);
    });
//setp-7
// goto Tools → NuGet Package Manager → Package Manager Console-> Default Project = Entities Project
// Install-package Microsoft.EntityFrameworkCore.Tools
// Add-Migration Initial
// Update-Database -verbose
var app = builder.Build();

if(app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
app.UseStaticFiles();
//app.UseRouting();
app.MapControllers();

using var db = new PersonsDbContext(
    new DbContextOptionsBuilder<PersonsDbContext>()
        .UseSqlServer(builder.Configuration["ConnectionStrings:DefaultConnection"])
        .Options);
var persons = await db.Persons
    .OrderBy(c => c.PersonName)
    .Take(5)
    .ToListAsync();
foreach (var person in persons)
{
    Console.WriteLine($"Person: {person.PersonName}, Country: {person.CountryID}");
}
app.Run();
