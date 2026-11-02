using CitiesManager.Core.DTO;
using CitiesManager.Core.Identity;
using CitiesManager.Core.ServiceContracts;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CitiesManager.Core.Services
{
    // 4 - پیاده سازی jwtService
    public class JwtService : IJwtService
    {
        // وابستگی به IConfiguration برای خواندن تنظیمات JWT از appsettings.json
        // (کلید مخفی، صادرکننده، دریافت‌کننده و مدت اعتبار توکن)
        private readonly IConfiguration _configuration;

        // سازنده کلاس: IConfiguration به صورت Dependency Injection تزریق می‌شود
        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// ساخت توکن JWT برای کاربر احراز هویت شده
        /// </summary>
        /// <param name="user">کاربری که توکن برای او صادر می‌شود</param>
        /// <returns>پاسخ شامل توکن، ایمیل، نام و زمان انقضا</returns>
        public AuthenticationResponse CreateJwtToken(ApplicationUser user)
        {
            // محاسبه زمان انقضای توکن بر اساس مقدار EXPIRATION_MINUTES در تنظیمات
            // DateTime.UtcNow برای جلوگیری از مشکلات منطقه زمانی (TimeZone) استفاده می‌شود
            DateTime expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:EXPIRATION_MINUTES"]));

            // تعریف Claimها (ادعاها) - اطلاعاتی که داخل Payload توکن قرار می‌گیرند
            // این اطلاعات در سمت کلاینت قابل خواندن هستند (Base64)، پس هرگز داده حساس (مثل رمز عبور) قرار ندهید
            Claim[] claims = new Claim[] {
            // Sub (Subject): شناسه یکتای کاربر (اینجا Id کاربر) - Claim استاندارد JWT
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), //Subject (user id)

            // Jti (JWT ID): شناسه یکتای خود توکن - برای جلوگیری از تکرار و امکان Blacklist کردن توکن
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), //JWT unique ID

            // Iat (Issued At): زمان صدور توکن به صورت Unix Timestamp (ثانیه‌های سپری شده از 1970)
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()), //Issued at (date and time of token generation)

            // NameIdentifier: شناسه نام کاربر (اینجا ایمیل) - ASP.NET Core از این Claim برای شناسایی کاربر استفاده می‌کند
            // دسترسی به آن در کنترلرها: User.FindFirstValue(ClaimTypes.NameIdentifier)
            new Claim(ClaimTypes.NameIdentifier, user.Email), //Unique name identifier of the user (Email)

            // Name: نام نمایشی کاربر - قابل دسترسی از طریق User.Identity.Name
            new Claim(ClaimTypes.Name, user.PersonName) //Name of the user
        };

            // ایجاد کلید متقارن (Symmetric) از رشته Secret Key موجود در تنظیمات
            // این کلید برای امضا و اعتبارسنجی توکن استفاده می‌شود و باید حداقل 32 بایت (256 بیت) طول داشته باشد
            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])
            );

            // تعیین الگوریتم امضا: HmacSha256 (HMAC + SHA-256) - یک الگوریتم متقارن
            // در این روش، همان کلیدی که برای امضا استفاده می‌شود، برای اعتبارسنجی هم به کار می‌رود
            // برای محیط‌های توزیع‌شده بهتر است از الگوریتم نامتقارن مانند RS256 استفاده شود
            SigningCredentials signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // ساخت شیء توکن JWT با پارامترهای:
            // Issuer: صادرکننده توکن (معمولاً آدرس API)
            // Audience: دریافت‌کننده مورد نظر توکن (معمولاً آدرس کلاینت یا خود API)
            // Claims: ادعاهای تعریف شده در بالا
            // Expires: زمان انقضا (پس از آن توکن نامعتبر است)
            // SigningCredentials: اطلاعات امضای توکن
            JwtSecurityToken tokenGenerator = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: expiration,
                signingCredentials: signingCredentials
            );

            // تبدیل شیء JwtSecurityToken به رشته نهایی توکن (فرمت Header.Payload.Signature)
            // این رشته همان چیزی است که به کلاینت ارسال می‌شود
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            string token = tokenHandler.WriteToken(tokenGenerator);

            // بازگرداندن پاسخ شامل توکن و اطلاعات تکمیلی کاربر
            // Expiration برای اطلاع کلاینت از زمان تمدید توکن (با Refresh Token) مفید است
            return new AuthenticationResponse()
            {
                Token = token,
                Email = user.Email,
                PersonName = user.PersonName,
                Expiration = expiration
            };
        }
    }
}

