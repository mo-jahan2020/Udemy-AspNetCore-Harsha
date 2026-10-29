using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SabtAhval.Models.Dtos;
using SabtAhval.Services;
using SabtAhval.Services.Interfaces;
//using Serilog.Core;

namespace SabtAhval.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProxyController : ControllerBase
    {
        private readonly ApiKeyService _apiClientService;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IIpWhitelistService _ipAuthService;
        private readonly ITargetApiServerService _serverService;
        private readonly IJsonPlaceholderService _jsonPlaceholderService;

        public ProxyController(ApiKeyService apiClientService, IIpWhitelistService ipAuthService, ITargetApiServerService serverService, IJsonPlaceholderService jsonPlaceholderService, ILoggerFactory loggerFactory)
        {
            _apiClientService = apiClientService;
            _loggerFactory = loggerFactory;
            _ipAuthService = ipAuthService;
            _serverService = serverService;
            _jsonPlaceholderService = jsonPlaceholderService;
        }

        [HttpGet("data")]
        public async Task<IActionResult> GetData([FromHeader(Name = "X-Api-Key")] string apiKey)
        {
            var server = await _serverService.GetDefaultAsync();

            if (server is null)
                return NotFound(new { message = "No active target API server found." });

            int? clientId = 1; // در عمل از توکن یا هدر گرفته می‌شود

            // IP کاربر را از Connection استخراج کن
            string? ip = HttpContext.Connection.RemoteIpAddress?.ToString();

            bool isAllowed = await _ipAuthService.IsIpAllowedAsync(ip!, clientId);

            if (!isAllowed)
                return StatusCode(403, new { message = "IP is not allowed." });

            var client = await _apiClientService.GetByApiKeyAsync(apiKey);

            if (client is null)
            {
                await AuditService.LogRejectedAsync(_loggerFactory, "Faild");
                return Unauthorized(new { message = "Invalid or inactive API key." });
            }
            // ادامه منطق: ارسال درخواست به TargetApiServer با استفاده از client.TargetApiServer.BaseUrl
            await AuditService.LogSuccessAsync(_loggerFactory, "OK");

            return Ok(new { client = client.Username, target = server.BaseUrl });

        }
        // 2️⃣ GET: api/posts/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetPostById(int id)
        {
            if (id <= 0)
                return BadRequest(new { message = "Id must be greater than zero." });

            try
            {
                var post = await _jsonPlaceholderService.GetPostByIdAsync(id);

                if (post is null)
                    return NotFound(new { message = $"Post with id {id} was not found." }); // 404

                return Ok(post); // 200
            }
            catch (HttpRequestException ex)
            {
                await AuditService.LogSuccessAsync(_loggerFactory, "Error while fetching post {Id}.");
                return StatusCode(StatusCodes.Status502BadGateway,
                    new { message = "Failed to reach target API.", detail = ex.Message });
            }
        }

        // 3️⃣ POST: api/posts
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

                // 201 Created + Location header
                return CreatedAtAction(
                    nameof(GetPostById),
                    new { id = created.id},
                    created);
            }
            catch (HttpRequestException ex)
            {
                await AuditService.LogSuccessAsync(_loggerFactory,"Error while creating post.");
                return StatusCode(StatusCodes.Status502BadGateway,
                    new { message = "Failed to reach target API.", detail = ex.Message });
            }
        }
    }

}

