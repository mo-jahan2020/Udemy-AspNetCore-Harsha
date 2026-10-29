using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SabtAhval.Data;
using SabtAhval.Middlewares;
using SabtAhval.Services;
using SabtAhval.Services.Interfaces;
using Serilog;
using Microsoft.OpenApi;      // ⚠️ در OpenApi v2 دیگر Microsoft.OpenApi.Models وجود ندارد
using SabtAhval.Common;       // برای Constants
//98AHGG^%^aassjd12
var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((HostBuilderContext context, IServiceProvider services, LoggerConfiguration loggerConfiguration) => {

    loggerConfiguration
    .ReadFrom.Configuration(context.Configuration) //read configuration settings from built-in IConfiguration
    .ReadFrom.Services(services); //read out current app's services and make them available to serilog
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped<IIpWhitelistService,IpWhitelistService>();
builder.Services.AddScoped<IApiKeyService,ApiKeyService>();
builder.Services.AddScoped<IAuditService,AuditService>();

builder.Services.AddScoped<ITargetApiServerService,TargetApiServerService>();
builder.Services.AddHttpClient<IJsonPlaceholderService,JsonPlaceholderService>(client =>
{
    client.BaseAddress = new Uri("https://jsonplaceholder.typicode.com/");
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // ۱) تعریف طرح امنیتی: دکمه Authorize (قفل 🔒) در Swagger ظاهر می‌شود
    options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Name = Constants.ApiKeyHeader,   // "X-Api-Key" ← اسمِ هدر، نه مقدار کلید!
        Description = "کلید API خود را وارد کنید. مثال: 98AHGG^%^aassjd12",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });

    // ۲) الزام امنیتی — در Swashbuckle 10 به‌صورت lambda است:
    //    Swashbuckle سند (document) را در لحظه تولید، به ما می‌دهد تا به ارجاع پاس دهیم
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("ApiKey", document)] = new List<string>()
    });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseMiddleware<ExceptionHandlingMiddleware>();  // ۱) بیرونی‌ترین
app.UseMiddleware<IpWhitelistMiddleware>();       // ۲)
app.UseMiddleware<ApiKeyValidationMiddleware>();  // ۳)
app.UseAuthorization();

app.MapControllers();

app.Run();
