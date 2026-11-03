using JwtDemoMVC.Models;
using JwtDemoMVC.Repositories;
using JwtDemoMVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JwtDemoMVC.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [Authorize]
    public class SettingsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthService _authService;

        public SettingsController(IUnitOfWork unitOfWork, IAuthService authService)
        {
            _unitOfWork = unitOfWork;
            _authService = authService;
        }

        /// <summary>لیست همه‌ی آی پی ها</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<AllowedIp>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAll()
            => Ok(await _unitOfWork.AllowedIps.GetAllAsync());

        /// <summary>دریافت یک آی پی  با شناسه</summary>
        /// <param name="id">شناسه‌ی شخص</param>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(AllowedIp), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetById(int id)
        {
            var allowIP = await _unitOfWork.AllowedIps.GetByIdAsync(id);
            return allowIP is null
                ? NotFound(new { message = "آی پی با این شناسه یافت نشد." })
                : Ok(allowIP);
        }

        [HttpPost]
        [ProducesResponseType(typeof(AllowedIp), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create([FromBody] AllowedIp allowedIp)
        {

            allowedIp.CreatedAtUtc = DateTime.UtcNow;
            allowedIp.ExpiresAtUtc = DateTime.UtcNow.AddYears(1);
            await _unitOfWork.AllowedIps.AddAsync(allowedIp);
            await _unitOfWork.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = allowedIp.Id }, allowedIp);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(AllowedIp), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Update(int id, [FromBody] AllowedIp allowedIp)
        {
            var existing = await _unitOfWork.AllowedIps.GetByIdAsync(id);
            if (existing is null)
                return NotFound(new { message = "آی پی با این شناسه یافت نشد." });

            existing.IpAddress = allowedIp.IpAddress;
            existing.Note = allowedIp.Note;
            existing.CreatedAtUtc = DateTime.UtcNow;
            existing.ExpiresAtUtc = DateTime.UtcNow.AddYears(1);
            _unitOfWork.AllowedIps.Update(existing);
            await _unitOfWork.SaveChangesAsync();
            return Ok(existing);
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Delete(int id)
        {
            var allowIp = await _unitOfWork.AllowedIps.GetByIdAsync(id);
            if (allowIp is null)
                return NotFound(new { message = "شخصی با این شناسه یافت نشد." });

            _unitOfWork.AllowedIps.Remove(allowIp);
            await _unitOfWork.SaveChangesAsync();
            return NoContent();
        }

    }
}
