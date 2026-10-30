using Microsoft.AspNetCore.Mvc;
using SabtAhval.Data;
using SabtAhval.Services.Interfaces;

namespace SabtAhval.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        private const string HealthCheckClientName = "HealthCheck";

        private readonly AppDbContext _db;
        private readonly ITargetApiServerService _serverService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<HealthController> _logger;

        public HealthController( AppDbContext db, ITargetApiServerService serverService,  IHttpClientFactory httpClientFactory, ILogger<HealthController> logger)
        {
            _db = db;
            _serverService = serverService;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        // ۱) GET /Health — زنده‌بودن خود گیت‌وی (سریع؛ فقط دیتابیس)
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var checks = new Dictionary<string, string>();
            bool isHealthy = true;

            try
            {
                bool dbOk = await _db.Database.CanConnectAsync();
                checks["database"] = dbOk ? "Healthy" : "Unhealthy";
                if (!dbOk) isHealthy = false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed: database is unreachable.");
                checks["database"] = "Unhealthy";
                isHealthy = false;
            }

            var result = new
            {
                status = isHealthy ? "Healthy" : "Unhealthy",
                checkedAtUtc = DateTime.UtcNow,
                checks
            };

            return isHealthy
                ? Ok(result)
                : StatusCode(StatusCodes.Status503ServiceUnavailable, result);
        }

        // ۲) GET /Health/target — در دسترس بودن اولین API Server ثبت‌شده (فیلد BaseUrl)
        [HttpGet("target")]
        public async Task<IActionResult> GetTarget()
        {
            var checks = new Dictionary<string, string>();
            bool isHealthy = true;

            // الف) اول دیتابیس — چون لیست سرورها از همان جا خوانده می‌شود
            try
            {
                bool dbOk = await _db.Database.CanConnectAsync();
                checks["database"] = dbOk ? "Healthy" : "Unhealthy";
                if (!dbOk) isHealthy = false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed: database is unreachable.");
                checks["database"] = "Unhealthy";
                isHealthy = false;
            }

            // ب) سرور مقصد — فقط وقتی دیتابیس سالم است معنا دارد
            if (isHealthy)
            {
                var server = await _serverService.GetDefaultAsync();   // همان سرویسی که خود پروکسی استفاده می‌کند

                if (server is null || string.IsNullOrWhiteSpace(server.BaseUrl))
                {
                    checks["targetApiServer"] = "Unhealthy (no active server registered)";
                    isHealthy = false;
                }
                else
                {
                    try
                    {
                        var client = _httpClientFactory.CreateClient(HealthCheckClientName);

                        // HEAD = سبک‌ترین درخواست؛ فقط وضعیت، بدون دانلود بدنه
                        using var response = await client.SendAsync(
                            new HttpRequestMessage(HttpMethod.Head, server.BaseUrl));

                        // هر پاسخ HTTP (حتی 404/405) یعنی سرور زنده و پاسخ‌گوست
                        checks["targetApiServer"] = (int)response.StatusCode switch
                        {
                            >= 200 and < 300 => $"Healthy ({(int)response.StatusCode})",
                            >= 500 => $"Unhealthy ({(int)response.StatusCode})",
                            _ => $"Reachable ({(int)response.StatusCode})"
                        };

                        if ((int)response.StatusCode >= 500)
                            isHealthy = false;
                    }
                    catch (Exception ex)   // Timeout، DNS، Connection Refused و ...
                    {
                        _logger.LogWarning(ex, "Health check: target server {Url} is unreachable.", server.BaseUrl);
                        checks["targetApiServer"] = "Unreachable";
                        isHealthy = false;
                    }
                }
            }

            var result = new
            {
                status = isHealthy ? "Healthy" : "Unhealthy",
                checkedAtUtc = DateTime.UtcNow,
                checks
            };

            return isHealthy
                ? Ok(result)
                : StatusCode(StatusCodes.Status503ServiceUnavailable, result);
        }
    }
}