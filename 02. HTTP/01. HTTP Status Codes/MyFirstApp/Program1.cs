/*در// Minimal API، معمولاً به جای app.Run(handler) از متدهای Map* استفاده می‌کنیم
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// ✅ این روش بهتر و رایج‌تر است
app.MapGet("/", async (HttpContext context) =>
{
    await context.Response.WriteAsync("Hello World");
});

app.MapGet("/api/users", () =>
{
    return new[] { "Ali", "Reza", "Sara" };
});

// ✅ سرور را شروع می‌کند
app.Run();
*/