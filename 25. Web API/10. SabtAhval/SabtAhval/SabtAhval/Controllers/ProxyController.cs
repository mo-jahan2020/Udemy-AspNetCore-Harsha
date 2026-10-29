using Microsoft.AspNetCore.Mvc;
using SabtAhval.Common;                 // Constants
using SabtAhval.Models.Dtos;
using SabtAhval.Models.Entities;        // ApiClient
using SabtAhval.Services.Interfaces;

namespace SabtAhval.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProxyController : ControllerBase
    {
        private readonly ITargetApiServerService _serverService;
        private readonly IJsonPlaceholderService _jsonPlaceholderService;
        private readonly IAuditService _auditService;

        // ⬇️ دیگر IApiKeyService و IIpWhitelistService اینجا کار ندارند — میان‌افزار انجامشان داد
        public ProxyController(
            ITargetApiServerService serverService,
            IJsonPlaceholderService jsonPlaceholderService,
            IAuditService auditService)
        {
            _serverService = serverService;
            _jsonPlaceholderService = jsonPlaceholderService;
            _auditService = auditService;
        }

        [HttpGet("data")]
        public async Task<IActionResult> GetData()
        {
            // ۱) کاربر احرازشده را از Items بخوان (بدون کوئری دوباره از دیتابیس)
            var client = HttpContext.Items[Constants.ApiClientItemKey] as ApiClient;

            if (client is null)   // دفاعی: اگر روزی میان‌افزار حذف/تغییر کرد
                return Unauthorized(new { message = "API client is not authenticated." });

            // ۲) سرور مقصد فعال از دیتابیس
            var server = await _serverService.GetDefaultAsync();
            if (server is null)
                return NotFound(new { message = "No active target API server found." });

            // ۳) ثبت لاگ با هویت و IP واقعی کاربر (نه مقدار هاردکد)
            string? ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            await _auditService.LogSuccessAsync($"Client={client.Username}, IP={ip}, Target={server.BaseUrl}");

            return Ok(new
            {
                authenticatedUser = client.Username,
                clientIp = ip,
                targetServer = server.BaseUrl
            });
        }

        // GetPostById و CreatePost بدون تغییر باقی می‌مانند
    }
}