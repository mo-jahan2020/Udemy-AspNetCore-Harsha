var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

var app = builder.Build();


app.UseStaticFiles();
app.Map("/", async context =>
{
    // write the value of the configuration key "myKey" to the response
    await context.Response.WriteAsync(app.Configuration["mykEY"] + "\n");
    // write the value of the configuration key "MyKey" to the response with string interpolation
    await context.Response.WriteAsync(app.Configuration.GetValue<string>("MyKey") + "\n");
    // write the value of the configuration key "x" to the response with a default value of 10
    // چون پیدا نمیشود 10 را مینویسد
    await context.Response.WriteAsync(app.Configuration.GetValue<int>("x", 10) + "\n");

});

app.MapControllers();

app.Run();

