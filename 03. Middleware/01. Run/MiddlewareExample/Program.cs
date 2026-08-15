var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.Use(async (HttpContext context,RequestDelegate next) =>
{
    await context.Response.WriteAsync("Hello");
    // اگر این خط نباشد فقط سلام چاپ میشود و دیگر به بعدی نمی رود
    await next(context);
});

app.Run(async (HttpContext context) =>
{
    await context.Response.WriteAsync("Hello again");
});

app.Run();
//
