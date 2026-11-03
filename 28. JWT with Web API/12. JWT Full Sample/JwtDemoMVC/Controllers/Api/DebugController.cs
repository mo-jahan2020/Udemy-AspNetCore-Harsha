// ═══════════════════════════════════════════════════════════════════════════
//  DebugController.cs — ابزار آموزشی برای دیدن «کدام بررسی اعتبارسنجی شکست خورد»
//  فقط در محیط Development فعال است؛ در Production پاسخ 404 می‌دهد.
//
//  انگیزه: وقتی سرور یک توکن را با 401 رد می‌کند، کلاینت نمی‌بیند دلیلش چه بود —
//  امضا؟ Issuer؟ Audience؟ انقضا؟ این Endpoint همان TokenValidationParameters
//  گام 10 در Program.cs را اجرا می‌کند و گزارش گام‌به‌گام برمی‌گرداند.
// ═══════════════════════════════════════════════════════════════════════════

using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using System.Text.RegularExpressions;
using JwtDemoMVC.Models;
using JwtDemoMVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace JwtDemoMVC.Controllers.Api;

[ApiController]
[Route("api/debug")]
[Produces("application/json")]
public class DebugController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;

    public DebugController(IConfiguration configuration, IHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
    }

    /// <summary>
    /// بررسی آموزشی توکن — گزارش می‌دهد کدام بررسی (امضا / Issuer / Audience / انقضا) شکست خورده است.
    /// </summary>
    /// <remarks>
    /// فقط در Development فعال است. توکن معتبر → valid=true؛ در غیر این صورت
    /// failedCheck و errorCode (مثل IDX10214) دلیل دقیق را نشان می‌دهند.
    /// همین بررسی‌ها در Program.cs (گام 10) به‌صورت خودکار و بی‌صدا انجام می‌شود و
    /// نتیجه‌اش همان 401هایی است که Swagger نشان می‌دهد.
    /// </remarks>
    [HttpPost("token-check")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult CheckToken([FromBody] TokenCheckRequest request)
    {
        if (!_environment.IsDevelopment())
            return NotFound();

        // ۱) پارسِ «بدون اعتبارسنجی» — محتوای سه‌بخشی توکن را نشان می‌دهد.
        //    نکته‌ی آموزشی: چون Header و Payload فقط Base64 هستند، حتی توکنِ جعلی هم
        //    کاملاً «خواندنی» است؛ راز بودن محتوا هرگز هدف JWT نیست (توکن امضا می‌شود، رمز نمی‌شود).
        object? header = null;
        object? payload = null;
        try
        {
            var raw = new JwtSecurityTokenHandler().ReadJwtToken(request.Token);
            header = JsonDocument.Parse(raw.Header.SerializeToJson()).RootElement;
            payload = JsonDocument.Parse(raw.Payload.SerializeToJson()).RootElement;
        }
        catch
        {
            // توکن قابل پارس نیست → در گزارش پایین با failedCheck=«ساختار توکن» دیده می‌شود
        }

        // ۲) اعتبارسنجی کامل — دقیقاً همان پارامترهای گام 10 در Program.cs
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = _configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = TokenService.CreateSigningKey(_configuration["Jwt:Key"]!),
        };

        try
        {
            var principal = new JwtSecurityTokenHandler().ValidateToken(request.Token, parameters, out _);
            return Ok(new
            {
                valid = true,
                message = "همه‌ی بررسی‌ها پاس شد: امضا ✓  Issuer ✓  Audience ✓  Lifetime ✓",
                header,
                payload,
                claims = principal.Claims.Select(c => new { type = c.Type, value = c.Value }),
            });
        }
        catch (SecurityTokenInvalidSignatureException ex)
        {
            return Ok(Report(header, payload, "امضای دیجیتال (Signature)", ex,
                "توکن با کلید رمزی سرور امضا نشده یا بعد از امضا دستکاری شده است. " +
                "این «دیوار واقعی» جلوی جعل توکن است؛ iss/aud درستِ توکنِ جعلی کمکی نمی‌کند."));
        }
        catch (SecurityTokenInvalidIssuerException ex)
        {
            return Ok(Report(header, payload, "Issuer", ex,
                $"مقدار iss داخل توکن با Jwt:Issuer تنظیمات ({_configuration["Jwt:Issuer"]}) یکی نیست. " +
                "توجه: ValidateIssuer فقط توکنِ رسیده را فیلتر می‌کند و ربطی به «ساختنِ» توکن ندارد."));
        }
        catch (SecurityTokenInvalidAudienceException ex)
        {
            return Ok(Report(header, payload, "Audience", ex,
                $"مقدار aud داخل توکن با Jwt:Audience تنظیمات ({_configuration["Jwt:Audience"]}) یکی نیست. " +
                "توجه: aud ادعای داخل توکن است، نه آدرس فراخوان؛ سرور آدرس فرستنده را بررسی نمی‌کند."));
        }
        catch (SecurityTokenExpiredException ex)
        {
            return Ok(Report(header, payload, "انقضا (Lifetime)", ex, "توکن منقضی شده است."));
        }
        catch (SecurityTokenNotYetValidException ex)
        {
            return Ok(Report(header, payload, "انقضا (Lifetime)", ex, "nbf توکن هنوز فرا نرسیده است."));
        }
        catch (Exception ex)
        {
            return Ok(Report(header, payload, "ساختار توکن", ex,
                "توکن قابل پارس یا اعتبارسنجی نیست (شکل سه‌بخشی Header.Payload.Signature را دارد؟)"));
        }
    }

    // ساخت گزارش یکسان برای همه‌ی حالت‌های رد شدن + استخراج کد IDX از پیام خطا
    private static object Report(object? header, object? payload, string check, Exception ex, string explanation)
    {
        var code = Regex.Match(ex.Message, @"IDX\d{5}").Value;
        return new
        {
            valid = false,
            failedCheck = check,
            errorCode = string.IsNullOrEmpty(code) ? null : code,
            detail = ex.Message,
            explanation,
            header,
            payload,
        };
    }
}
