using Microsoft.AspNetCore.Mvc;
using SabtAhval.Common;
using SabtAhval.Models.Dtos;
using SabtAhval.Models.Entities;
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

        // دقت کن: دیگر IApiKeyService و IIpWhitelistService اینجا لازم نیست!
        // چک IP و کلید حالا کار Middleware هاست، نه کنترلر
        public ProxyController(ITargetApiServerService serverService,
                               IJsonPlaceholderService jsonPlaceholderService,
                               IAuditService auditService)
        {
            _serverService = serverService;
            _jsonPlaceholderService = jsonPlaceholderService;
            _auditService = auditService;
        }

        // 1️⃣ GET: /Proxy/data
        [HttpGet("data")]
        public async Task<IActionResult> GetData()
        {
            // Middleware کلید را تأیید کرده و client را در کیف گذاشته؛ فقط برمی‌داریم
            var client = HttpContext.Items[Constants.ApiClientItemKey] as ApiClient;

            // چک دفاعی: در مسیرهای عادی هیچ‌وقت نباید null باشد
            if (client is null)
                return Unauthorized(new { message = "API key is missing or invalid." });

            // سرور اختصاصی همین کاربر (که Include شده) وگرنه سرور پیش‌فرض فعال
            var server = client.TargetApiServer ?? await _serverService.GetDefaultAsync();

            if (server is null)
                return NotFound(new { message = "No active target API server found." });

            await _auditService.LogSuccessAsync($"Client '{client.Username}' reached /Proxy/data");

            return Ok(new { client = client.Username, target = server.BaseUrl });
        }

        // 2️⃣ GET: /Proxy/5  — بدون تغییر
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetPostById(int id)
        {
            if (id <= 0)
                return BadRequest(new { message = "Id must be greater than zero." });

            try
            {
                var post = await _jsonPlaceholderService.GetPostByIdAsync(id);

                if (post is null)
                    return NotFound(new { message = $"Post with id {id} was not found." });

                return Ok(post);
            }
            catch (HttpRequestException)
            {
                await _auditService.LogRejectedAsync($"Failed to fetch post {id} from target API.");
                return StatusCode(StatusCodes.Status502BadGateway,
                    new { message = "Failed to reach target API." });
            }
        }

        // 3️⃣ POST: /Proxy — بدون تغییر
        [HttpPost]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostRequestDto request)
        {
            if (request is null)
                return BadRequest(new { message = "Request body is required." });

            if (string.IsNullOrWhiteSpace(request.title))
                return BadRequest(new { message = "Title is required." });

            if (string.IsNullOrWhiteSpace(request.body))
                return BadRequest(new { message = "Body is required." });

            try
            {
                var created = await _jsonPlaceholderService.CreatePostAsync(request);

                if (created is null)
                    return StatusCode(StatusCodes.Status502BadGateway,
                        new { message = "Target API did not return a valid response." });

                return CreatedAtAction(
                    nameof(GetPostById),
                    new { id = created.id },
                    created);
            }
            catch (HttpRequestException)
            {
                await _auditService.LogRejectedAsync("Failed to create post on target API.");
                return StatusCode(StatusCodes.Status502BadGateway,
                    new { message = "Failed to reach target API." });
            }
        }
    }
}