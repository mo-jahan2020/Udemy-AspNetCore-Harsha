var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
//این pipeline موقعی اجرا میشود که شرط برقرار باشد
app.UseWhen(
    //شرط
    context => context.Request.Query.ContainsKey("username"),
    //کاری که در صورت برقراری شرط باید انجام شود
    app =>
    {
        app.Use(async (context, next) =>
        {
            await context.Response.WriteAsync("Hello from Middleware branch");
            await next();
        });
    });

app.Run(async context =>
{
    await context.Response.WriteAsync("Hello from middleware at main chain");
});

app.Run();
