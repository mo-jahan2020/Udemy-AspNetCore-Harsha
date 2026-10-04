using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Serilog;
using TextToSqlApi.Data;
using TextToSqlApi.Middleware;
using TextToSqlApi.Models.Options;
using TextToSqlApi.Repositories;
using TextToSqlApi.Services;

// پیکربندی Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables()
        .Build())
    .CreateLogger();

try
{
    Log.Information("Starting TextToSqlApi service...");

    var builder = WebApplication.CreateBuilder(args);

    // جایگزینی ILogger پیش‌فرض با Serilog
    builder.Host.UseSerilog();

    // بارگذاری Options از appsettings.json (با override از Environment Variables)
    builder.Services.Configure<OllamaOptions>(builder.Configuration.GetSection(OllamaOptions.SectionName));
    builder.Services.Configure<VannaOptions>(builder.Configuration.GetSection(VannaOptions.SectionName));
    builder.Services.Configure<AppOptions>(builder.Configuration.GetSection(AppOptions.SectionName));

    // ثبت DbContext برای دیتابیس سشن
    var sessionConnStr = builder.Configuration.GetConnectionString("SessionDatabase")
        ?? throw new InvalidOperationException("ConnectionStrings:SessionDatabase is not configured.");

    builder.Services.AddDbContext<SessionDbContext>(options =>
        options.UseSqlServer(sessionConnStr, sql =>
        {
            sql.MigrationsAssembly(typeof(SessionDbContext).Assembly.FullName);
            sql.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null);
        }));

    // ثبت HttpClient برای سرویس‌های خارجی
    builder.Services.AddHttpClient<IOllamaService, OllamaService>();
    builder.Services.AddHttpClient<IVannaService, VannaService>();

    // ثبت Repository و Service ها
    builder.Services.AddScoped<ISessionRepository, SessionRepository>();
    builder.Services.AddScoped<ISessionService, SessionService>();
    builder.Services.AddScoped<ISqlService, SqlService>();
    builder.Services.AddScoped<ITextToSqlService, TextToSqlService>();

    // کنترلرها با suppress ModelState invalid filter (چون خودمان مدیریت می‌کنیم)
    builder.Services.AddControllers()
        .AddJsonOptions(o =>
        {
            o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            o.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        });

    // ProblemDetails پیش‌فرض برای خطاها
    builder.Services.AddProblemDetails(opts =>
    {
        opts.CustomizeProblemDetails = ctx =>
        {
            ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier;
            ctx.ProblemDetails.Extensions["timestamp"] = DateTime.UtcNow;
        };
    });

    // CORS
    var appOpts = builder.Configuration.GetSection(AppOptions.SectionName).Get<AppOptions>() ?? new AppOptions();
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            if (appOpts.CorsOrigins.Length == 0)
            {
                policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            }
            else
            {
                policy.WithOrigins(appOpts.CorsOrigins).AllowAnyMethod().AllowAnyHeader();
            }
        });
    });

    // Swagger / OpenAPI
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Text-to-SQL API",
            Version = "v1",
            Description = "Convert natural-language prompts to T-SQL queries using Ollama + Vanna.ai",
            Contact = new OpenApiContact
            {
                Name = "TextToSqlApi Team"
            }
        });
    });

    // Health Checks
    builder.Services.AddHealthChecks()
        .AddSqlServer(
            connectionString: builder.Configuration.GetConnectionString("SessionDatabase")!,
            name: "SessionDatabase",
            tags: new[] { "db", "sql", "session" })
        .AddDbContextCheck<SessionDbContext>(
            name: "EF-Core SessionDbContext",
            tags: new[] { "ef", "db" });

    var app = builder.Build();

    // اعمال Migration خودکار در زمان استارت (اختیاری)
    if (bool.TryParse(builder.Configuration["App:ApplyMigrationsOnStartup"], out var applyMigrations) && applyMigrations)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SessionDbContext>();
        db.Database.Migrate();
        Log.Information("Migrations applied to SessionDatabase.");
    }

    // Middleware pipeline
    app.UseMiddleware<GlobalExceptionMiddleware>();
    app.UseSerilogRequestLogging();
    app.UseCors();

    // فعال‌سازی Swagger و Swagger UI در همه محیط‌ها (Development و Production)
    // این تنظیم بر اساس درخواست کاربر انجام می‌شود تا در زمان run، Swagger در دسترس باشد.
    app.UseSwagger(c =>
    {
        c.RouteTemplate = "swagger/{documentName}/swagger.json";
    });
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Text-to-SQL API v1");
        c.RoutePrefix = "swagger";           // آدرس: /swagger
        c.DisplayRequestDuration();
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        c.EnableDeepLinking();
        c.EnableFilter();
        c.ShowExtensions();
    });

    // (اختیاری) در محیط Development می‌توان تنظیمات اضافی داشت
    if (app.Environment.IsDevelopment())
    {
        // در محیط توسعه، لاگ‌های verbose تر فعال می‌شود
        Log.Information("Running in Development environment");
    }
    else
    {
        Log.Information("Running in {Env} environment", app.Environment.EnvironmentName);
    }

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    // Health Check endpoint سبک
    app.MapHealthChecks("/health");
    app.MapGet("/api/health/simple", () => Results.Ok(new { status = "ok", timestamp = DateTime.UtcNow }));

    // صفحه پیش‌فرض ریشه به Swagger هدایت شود
    app.MapGet("/", context =>
    {
        context.Response.Redirect("/swagger");
        return Task.CompletedTask;
    });

    var port = appOpts.Port;
    app.Urls.Add($"http://0.0.0.0:{port}");

    Log.Information("TextToSqlApi listening on port {Port}", port);
    Log.Information("Swagger UI available at: http://localhost:{Port}/swagger", port);
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Service terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
