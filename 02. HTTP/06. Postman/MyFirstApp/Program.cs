var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.Run(async (HttpContext context) =>
{
    context.Response.Headers["Content-type"] = "text/plain";
    if (context.Request.Headers.ContainsKey("AuthorizationKey"))
    {
        string auth = context.Request.Headers["AuthorizationKey"].ToString();
        await context.Response.WriteAsync($"AuthorizationKey={auth}");
    }
});

app.Run();
