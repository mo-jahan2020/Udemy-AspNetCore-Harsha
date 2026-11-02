// ═══════════════════════════════════════════════════════════════════════════
//  Persons.cshtml.cs — تنها صفحه‌ی اصلی برنامه (محافظت‌شده با JWT)
//  گام‌های این فایل: 13 و 14 (Authorization with JWT)
//  سایر گام‌ها: TokenService.cs (1 تا 8 و 15 و 16) — Program.cs (9 تا 12 و 21)
//               AuthService.cs (17 تا 20 و 22)
// ═══════════════════════════════════════════════════════════════════════════

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using JwtDemo.Models;
using JwtDemo.Repositories;
using JwtDemo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JwtDemo.Pages;

// ═══════════ گام 13 | Authorization with JWT: محافظت از صفحه با [Authorize] ═══════════
// این صفحه فقط برای کاربرانِ احراز هویت‌شده (دارای توکن JWT معتبر در کوکی یا هدر) باز است.
// اگر توکن معتبر نباشد، چالش (Challenge) اجرا می‌شود و کاربر به صفحه‌ی ورود هدایت می‌شود
// (OnChallenge در Program.cs). این ویژگی از پکیج Microsoft.AspNetCore.Authorization است.
[Authorize]
public class PersonsModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;

    public PersonsModel(IUnitOfWork unitOfWork, IAuthService authService, ITokenService tokenService)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
        _tokenService = tokenService;
    }

    // ───────── داده‌های جدول اشخاص ─────────
    public IReadOnlyList<Person> People { get; private set; } = Array.Empty<Person>();

    [BindProperty]
    public Person Person { get; set; } = new();

    public int? EditId { get; private set; }

    // ───────── داده‌های پنل آموزشی JWT ─────────
    public string? AccessToken { get; private set; }
    public string? TokenHeaderJson { get; private set; }
    public string? TokenPayloadJson { get; private set; }
    public DateTime? AccessExpiresAtUtc { get; private set; }
    public IReadOnlyList<Claim> TokenClaims { get; private set; } = Array.Empty<Claim>();
    public string? RefreshTokenMasked { get; private set; }
    public DateTime? RefreshExpiresAtUtc { get; private set; }

    // نام کاربریِ جاری از Claims توکن خوانده می‌شود (گام 14)
    public string CurrentUsername =>
        User.Identity?.Name
        ?? User.FindFirst(ClaimTypes.Name)?.Value
        ?? User.FindFirst("unique_name")?.Value
        ?? "-";

    public async Task OnGetAsync(int? editId)
    {
        EditId = editId;

        People = await _unitOfWork.Persons.GetAllAsync();

        // اگر حالت ویرایش است، مقدار فرم را با شخص انتخاب‌شده پر می‌کنیم
        if (editId is not null)
        {
            var editing = People.FirstOrDefault(p => p.Id == editId);
            if (editing is not null)
                Person = editing;
        }

        await LoadJwtPanelDataAsync();
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        if (!ModelState.IsValid)
        {
            await ReloadPageDataAsync();
            return Page();
        }

        await _unitOfWork.Persons.AddAsync(new Person
        {
            FirstName = Person.FirstName.Trim(),
            LastName = Person.LastName.Trim(),
            Age = Person.Age,
        });
        await _unitOfWork.SaveChangesAsync();

        return RedirectToPage(); // الگوی Post/Redirect/Get
    }

    public async Task<IActionResult> OnPostEditAsync()
    {
        var person = await _unitOfWork.Persons.GetByIdAsync(Person.Id);
        if (person is null)
            return RedirectToPage();

        if (!ModelState.IsValid)
        {
            EditId = person.Id;
            await ReloadPageDataAsync();
            return Page();
        }

        person.FirstName = Person.FirstName.Trim();
        person.LastName = Person.LastName.Trim();
        person.Age = Person.Age;
        _unitOfWork.Persons.Update(person);
        await _unitOfWork.SaveChangesAsync();

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var person = await _unitOfWork.Persons.GetByIdAsync(id);
        if (person is not null)
        {
            _unitOfWork.Persons.Remove(person);
            await _unitOfWork.SaveChangesAsync();
        }

        return RedirectToPage();
    }

    // دکمه‌ی «صدور توکن جدید» — تمدید دستی جفت توکن‌ها
    // (اعتبارسنجی و چرخش توکن در گام‌های 19 و 20 در AuthService انجام می‌شود)
    public async Task<IActionResult> OnPostRefreshTokenAsync()
    {
        await _authService.TryRefreshAsync(HttpContext);
        return RedirectToPage();
    }

    private async Task ReloadPageDataAsync()
    {
        People = await _unitOfWork.Persons.GetAllAsync();
        await LoadJwtPanelDataAsync();
    }

    // خواندن داده‌های توکن‌های جاری برای نمایش در «پنل آموزشی JWT»
    private async Task LoadJwtPanelDataAsync()
    {
        // اگر در همین درخواست «تمدید خودکار» انجام شده باشد (گام 21)، توکن‌های جدید
        // در HttpContext.Items قرار گرفته‌اند؛ در غیر این صورت از کوکی می‌خوانیم.
        var accessToken = HttpContext.Items["CurrentAccessToken"] as string
                         ?? Request.Cookies[AuthService.AccessTokenCookie];

        if (!string.IsNullOrEmpty(accessToken))
        {
            AccessToken = accessToken;
            try
            {
                // فقط پارس توکن برای نمایش — اعتبارسنجی واقعی قبلاً توسط JwtBearer انجام شده
                var jwt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);

                // ═══════════ گام 14 | Authorization with JWT: خواندن هویت کاربر از ClaimsPrincipal ═══════════
                // بعد از اعتبارسنجی توکن، هویت کاربر در User.Identity و User.Claims در دسترس است.
                // این Claims دقیقاً همان‌هایی هستند که در گام 6 داخل توکن قرار دادیم.
                // در صفحه، دو جدول نمایش داده می‌شود: Claims خامِ داخل توکن (jwt.Claims)
                // و Claims نهایی User — تفاوت نام‌ها به دلیل نگاشت ClaimTypeهاست (گام 6 و 9).
                TokenClaims = jwt.Claims.ToList();
                AccessExpiresAtUtc = jwt.ValidTo;

                TokenHeaderJson = JsonSerializer.Serialize(jwt.Header, new JsonSerializerOptions { WriteIndented = true });
                TokenPayloadJson = JsonSerializer.Serialize(jwt.Payload, new JsonSerializerOptions { WriteIndented = true });
            }
            catch
            {
                AccessToken = null;
            }
        }

        var rawRefreshToken = HttpContext.Items["CurrentRefreshToken"] as string
                              ?? Request.Cookies[AuthService.RefreshTokenCookie];

        if (!string.IsNullOrEmpty(rawRefreshToken))
        {
            // نمایش ماسک‌شده‌ی توکن تمدید — مقدار کامل امنیت دارد و نمایش داده نمی‌شود
            RefreshTokenMasked = rawRefreshToken.Length <= 16
                ? rawRefreshToken
                : $"{rawRefreshToken[..10]}…{rawRefreshToken[^6..]}";

            // زمان انقضای توکن تمدید از دیتابیس خوانده می‌شود (گام 17)
            var stored = await _unitOfWork.RefreshTokens
                .GetByTokenHashAsync(_tokenService.HashToken(rawRefreshToken));
            RefreshExpiresAtUtc = stored?.ExpiresAtUtc;
        }
    }
}
