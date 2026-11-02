// ═══════════════════════════════════════════════════════════════════════════
//  TokenService.cs — قلب آموزشی پیاده‌سازی JWT
//  گام‌های این فایل: 1، 2، 3 (JWT Basics) + 4 تا 8 (Generating JWT Tokens)
//                    + 15، 16 (Refresh Tokens)
//  سایر گام‌ها: Program.cs (9 تا 12 و 21) — AuthService.cs (17 تا 20 و 22)
//               Persons.cshtml.cs (13 و 14)
// ═══════════════════════════════════════════════════════════════════════════

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using JwtDemo.Models;
using Microsoft.IdentityModel.Tokens;

namespace JwtDemo.Services;

public class TokenService : ITokenService
{
    private readonly string _issuer;
    private readonly string _audience;
    private readonly string _secretKey;
    private readonly int _accessTokenMinutes;

    public TokenService(IConfiguration configuration)
    {
        // ═══════════ گام 1 | JWT Basics (مبانی JWT): ساختار توکن ═══════════
        // یک JWT رشته‌ای با سه بخش است که با نقطه از هم جدا می‌شوند:   xxxxx.yyyyy.zzzzz
        //   بخش 1) Header     : الگوریتم امضا و نوع توکن  →  { "alg": "HS256", "typ": "JWT" }
        //   بخش 2) Payload    : Claims (ادعاهای کاربر) مثل شناسه، نام کاربری و تاریخ انقضا
        //   بخش 3) Signature  : امضای دیجیتالِ دو بخش اول با کلید رمزی سرور
        // نکته مهم: توکن فقط «امضا» می‌شود نه «رمز» شدن؛ محتوای آن Base64 و قابل خواندن است
        // اما چون با کلید سرور امضا شده، قابل جعل نیست و سرور می‌تواند اصالتش را تشخیص دهد.
        //
        // ═══════════ گام 3 | JWT Basics: خواندن تنظیمات JWT از appsettings.json ═══════════
        // Issuer   = صادرکننده‌ی توکن (خودِ این سرور)
        // Audience = مصرف‌کننده‌ی مجاز توکن (مثلاً یک کلاینت Angular روی پورت 4200)
        // Key      = کلید رمزی که توکن با آن امضا/اعتبارسنجی می‌شود
        // EXPIRATION_MINUTES = عمر توکن دسترسی (طبق تنظیمات: فقط 1 دقیقه)
        _issuer = configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("تنظیم Jwt:Issuer در appsettings.json یافت نشد.");
        _audience = configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("تنظیم Jwt:Audience در appsettings.json یافت نشد.");
        _secretKey = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("تنظیم Jwt:Key در appsettings.json یافت نشد.");
        _accessTokenMinutes = int.Parse(configuration["Jwt:EXPIRATION_MINUTES"] ?? "1");
    }

    public string GenerateAccessToken(User user)
    {
        // ═══════════ گام 4 | Generating JWT Tokens (تولید توکن): ساخت کلید امضای متقارن ═══════════
        // الگوریتم HmacSha256 حداقل به کلیدی 256 بیتی (32 بایت) نیاز دارد.
        // چون کلید تنظیم‌شده در appsettings کوتاه‌تر است، اینجا به صورت «آموزشی» با تکرار
        // چرخه‌ای، طول کلید را به 32 بایت می‌رسانیم. (در پروژه واقعی از یک کلید طولانی
        // و تصادفی مثل یک GUID چندتایی یا رشته‌ی 64 کاراکتری استفاده کنید)
        var key = CreateSigningKey(_secretKey);

        // ═══════════ گام 5 | Generating JWT Tokens: ساخت اعتبارنامه‌ی امضا (SigningCredentials) ═══════════
        // مشخص می‌کند توکن با چه الگوریتمی (HmacSha256) و با چه کلیدی امضا شود.
        // همین اعتبارنامه است که بخش سوم توکن (Signature) را تولید می‌کند.
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // ═══════════ گام 2 | JWT Basics: مفهوم Claims ═══════════
        // Claim یک جفت (کلید، مقدار) است که «ادعایی» درباره‌ی کاربر در توکن ثبت می‌کند؛
        // مثلاً «شناسه‌ی کاربر این است» یا «نام کاربری او این است». سرور بعد از اعتبارسنجی
        // توکن، کاربر را فقط با همین Claims می‌شناسد — دیگر هیچ Session سمت سروری لازم نیست.

        // ═══════════ گام 6 | Generating JWT Tokens: ساخت لیست Claims کاربر ═══════════
        var claims = new List<Claim>
        {
            // شناسه و نام کاربری — هویت اصلی کاربر
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),

            // Jti: شناسه‌ی یکتای این توکن (برای ردیابی یا ابطال توکن در آینده)
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),

            // Iat: زمان صدور توکن (Unix Time)
            new(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64),
        };

        // ═══════════ گام 7 | Generating JWT Tokens: ساخت شیء توکن (JwtSecurityToken) ═══════════
        // Issuer صادرکننده، Audience مصرف‌کننده، Expires پایان اعتبار توکن است.
        // تمام این مقادیر بعداً در بخش Payload توکن قرار می‌گیرند و در اعتبارسنجی
        // (گام 10 در Program.cs) یکی‌یکی بررسی می‌شوند.
        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(_accessTokenMinutes), // طبق تنظیمات: 1 دقیقه
            signingCredentials: credentials);

        // ═══════════ گام 8 | Generating JWT Tokens: تبدیل به رشته‌ی نهایی توکن ═══════════
        // خروجی، همان رشته‌ی سه‌بخشی معروف «Header.Payload.Signature» است
        // که به کاربر داده می‌شود و او در درخواست‌های بعدی همراه خودش می‌فرستد.
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // ═══════════ گام 15 | Refresh Tokens (توکن تمدید): چرا به آن نیاز داریم؟ ═══════════
    // توکن دسترسی (Access Token) عمداً عمر کوتاهی دارد (اینجا فقط 1 دقیقه) تا اگر لو برود،
    // آسیب‌پذیری‌اش کم باشد؛ اما نمی‌توان از کاربر خواست هر 1 دقیقه مجدداً لاگین کند!
    // راه‌حل: «توکن تمدید» با عمر طولانی (اینجا 60 دقیقه) که فقط برای گرفتن توکن دسترسی
    // جدید به سرور فرستاده می‌شود و — برخلاف توکن دسترسی — در دیتابیس ذخیره می‌شود
    // و قابل ابطال است. با این کار خروج کاربر هم معنا پیدا می‌کند (گام 22).

    // ═══════════ گام 16 | Refresh Tokens: تولید مقدار امن و تصادفی ═══════════
    // برخلاف JWT، توکن تمدید ساختار مشخصی ندارد؛ فقط یک رشته‌ی کاملاً تصادفی و
    // پیش‌بینی‌ناپذیر است که با «مولد اعداد رمزنگاری‌امن» ساخته می‌شود (نه Random معمولی!)
    // تا مهاجم نتواند مقدار بعدی را حدس بزند.
    public string GenerateRefreshToken()
        => Convert.ToHexString(RandomNumberGenerator.GetBytes(64)); // 128 کاراکتر هگز — تصادفی امن

    // ═══════════ گام 17 | Refresh Tokens: هش کردن قبل از ذخیره در دیتابیس ═══════════
    // فقط هش توکن تمدید در دیتابیس نگه‌داری می‌شود؛ اگر دیتابیس لو برود،
    // مقدار واقعی توکن‌ها در دسترس مهاجم نیست. (ذخیره در AuthService انجام می‌شود)
    public string HashToken(string token)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    /// <summary>
    /// فقط «پارس» می‌کند و تاریخ انقضای توکن را بررسی می‌کند — بدون اعتبارسنجی امضا.
    /// برای تصمیم‌گیری سریع در OnMessageReceived (گام 21) استفاده می‌شود؛
    /// اعتبارسنجی کامل (امضا، Issuer، Audience و ...) را JwtBearer انجام می‌دهد (گام 10).
    /// </summary>
    public static bool IsTokenExpired(string token)
    {
        try
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            return jwt.ValidTo <= DateTime.UtcNow;
        }
        catch
        {
            return true; // توکن خراب است → منقضی فرض می‌شود
        }
    }

    /// <summary>
    /// ساخت کلید امضای متقارن (گام 4) — هم در «تولید» توکن (TokenService)
    /// و هم در «اعتبارسنجی» توکن (Program.cs) از همین متد استفاده می‌شود
    /// تا مطمئن باشیم کلیدِ امضا و کلیدِ اعتبارسنجی دقیقاً یکی است.
    /// </summary>
    public static SymmetricSecurityKey CreateSigningKey(string secret)
    {
        var bytes = Encoding.UTF8.GetBytes(secret);
        if (bytes.Length >= 32)
            return new SymmetricSecurityKey(bytes);

        // کلید کوتاه‌تر از 32 بایت (256 بیت) است → با تکرار چرخه‌ای به طول 32 بایت می‌رسانیم.
        // (الزام الگوریتم HMAC-SHA256 — در غیر این صورت خطای IDX10653 رخ می‌دهد)
        var extended = new byte[32];
        for (var i = 0; i < extended.Length; i++)
            extended[i] = bytes[i % bytes.Length];
        return new SymmetricSecurityKey(extended);
    }
}
