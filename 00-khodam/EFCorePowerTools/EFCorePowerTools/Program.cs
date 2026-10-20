using EFCorePowerTools.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");
using var db = new PersonsDatabaseContext();
//db.SaveChanges();
db.Persons.Add(new Person
{
    PersonId = Guid.NewGuid(),
    PersonName = "John Doe",
    Address = "123 Main St",
    Email = "a@a.com"
});
//db.SaveChanges();
var person1 = db.Persons.Find("John Doe");

var persons = await db.Persons
    .OrderBy(c => c.PersonName)
    .Take(5)
    .ToListAsync();
foreach (var person in persons)
{
    Console.WriteLine($"Person: {person.PersonName}, Country: {person.CountryId}");
}
app.Run();
