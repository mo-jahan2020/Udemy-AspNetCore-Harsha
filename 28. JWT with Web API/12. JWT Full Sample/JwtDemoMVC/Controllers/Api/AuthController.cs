using JwtDemoMVC.Models;
using JwtDemoMVC.Repositories;
using JwtDemoMVC.Security;
using JwtDemoMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace JwtDemoMVC.Controllers.Api;

/// <summary>
/// API احراز هویت — قابل تست در Swagger (آدرس /swagger).
/// این کنترلر «حالت اول» گام 11 را در عمل نشان می‌دهد: کلاینتِ API توکن را
/// در هدر «Authorization: Bearer ...» می‌فرستد و بدون هیچ کوکی‌ای احراز هویت می‌شود.
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;

    public AuthController(IUnitOfWork unitOfWork, IAuthService authService)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
    }

    /// <summary>ثبت‌نام و دریافت جفت توکن (دسترسی + تمدید)</summary>
    /// <remarks>
    /// پاسخ شامل accessToken (عمر 1 دقیقه) و refreshToken (عمر 60 دقیقه) است.
    /// توکن دسترسی را در دکمه‌ی Authorize وارد کنید و توکن تمدید را برای فراخوانی
    /// api/auth/refresh نگه دارید.
    /// </remarks>
    [HttpPost("register")]
    [ProducesResponseType(typeof(TokenPair), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (await _unitOfWork.Users.GetByUsernameAsync(request.Username) is not null)
            return BadRequest(new { message = "این نام کاربری قبلاً ثبت شده است." });

        var (hash, salt) = PasswordHasher.Hash(request.Password);
        var user = new User
        {
            Username = request.Username.Trim(),
            PasswordHash = hash,
            PasswordSalt = salt,
        };
        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // صدور جفت توکن (گام‌های 4 تا 8 برای توکن دسترسی + 16 و 17 برای تمدید)
        // — بدون کوکی؛ توکن‌ها در پاسخ JSON برمی‌گردند
        var tokens = await _authService.IssueTokensForApiAsync(user);
        return Ok(tokens);
    }

    /// <summary>ورود و دریافت جفت توکن (دسترسی + تمدید)</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenPair), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _unitOfWork.Users.GetByUsernameAsync(request.Username);

        if (user is null || !PasswordHasher.Verify(request.Password, user.PasswordSalt, user.PasswordHash))
            return Unauthorized(new { message = "نام کاربری یا رمز عبور اشتباه است." });

        var tokens = await _authService.IssueTokensForApiAsync(user);
        return Ok(tokens);
    }

    /// <summary>تمدید توکن — اعتبارسنجی توکن تمدید و چرخش آن (گام‌های 19 و 20)</summary>
    /// <remarks>هر بار فراخوانی، جفت توکن کاملاً جدید صادر و توکن قبلی باطل می‌شود (Rotation).</remarks>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(TokenPair), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        var tokens = await _authService.RefreshTokenAsync(request.RefreshToken);
        return tokens is null
            ? Unauthorized(new { message = "توکن تمدید نامعتبر، منقضی یا قبلاً باطل شده است." })
            : Ok(tokens);
    }

    /// <summary>خروج از API — ابطال توکن تمدید در دیتابیس (گام 22)</summary>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest request)
    {
        var revoked = await _authService.RevokeTokenAsync(request.RefreshToken);
        return revoked
            ? Ok(new { message = "توکن تمدید باطل شد." })
            : BadRequest(new { message = "توکن تمدید نامعتبر یا قبلاً باطل شده است." });
    }
}
