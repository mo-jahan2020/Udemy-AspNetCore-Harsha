using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SabtAhval.Data;
using SabtAhval.Services;
using SabtAhval.Services.Interfaces;
using Serilog;

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
builder.Services.AddScoped<IJsonPlaceholderService,JsonPlaceholderService>();
builder.Services.AddScoped<ITargetApiServerService,TargetApiServerService>();
builder.Services.AddHttpClient<ITargetApiServerService>(client =>
{
    client.BaseAddress = new Uri("https://jsonplaceholder.typicode.com/");
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapControllers();

app.Run();
