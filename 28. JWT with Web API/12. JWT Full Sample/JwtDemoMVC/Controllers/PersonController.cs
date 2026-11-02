// ═══════════════════════════════════════════════════════════════════════════
//  PersonController.cs — تنها صفحه‌ی اصلی برنامه (محافظت‌شده با JWT)
//  گام‌های این فایل: 13 و 14 (Authorization with JWT)
//  سایر گام‌ها: TokenService.cs (1 تا 8 و 15 و 16) — Program.cs (9 تا 12 و 21)
//               AuthService.cs (17 تا 20 و 22)
// ═══════════════════════════════════════════════════════════════════════════

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using JwtDemoMVC.Models;
using JwtDemoMVC.Repositories;
using JwtDemoMVC.Services;
using JwtDemoMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JwtDemoMVC.Controllers;

// ═══════════ گام 13 | Authorization with JWT: محافظت از کل کنترلر با [Authorize] ═══════════
// همه‌ی اکشن‌های این کنترلر فقط برای کاربرانِ احراز هویت‌شده باز است؛ یعنی درخواست باید
// توکن JWT معتبر داشته باشد: در کوکی مرورگر (ناوبری صفحات MVC) یا در هدر
// Authorization (فراخوانی API). اگر توکن معتبر نباشد، کاربر به صفحه‌ی ورود هدایت می‌شود
// (OnChallenge در Program.cs). این ویژگی از پکیج Microsoft.AspNetCore.Authorization است.
[Authorize]
[ApiExplorerSettings(IgnoreApi = true)] // این کنترلر View برمی‌گرداند؛ در Swagger نمایش داده نشود
public class PersonController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;

    public PersonController(IUnitOfWork unitOfWork, IAuthService authService, ITokenService tokenService)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
        _tokenService = tokenService;
    }

    // ═══════════ گام 14 | Authorization with JWT: خواندن هویت کاربر از ClaimsPrincipal ═══════════
    // بعد از اعتبارسنجی توکن، هویت کاربر در User.Identity و User.Claims در دسترس است.
    // این Claims دقیقاً همان‌هایی هستند که در گام 6 داخل توکن قرار دادیم و در پنل
    // آموزشی صفحه (کنارِ جدول Claims خام) نمایش داده می‌شوند.
    private string CurrentUsername =>
        User.Identity?.Name
        ?? User.FindFirst(ClaimTypes.Name)?.Value
        ?? User.FindFirst("unique_name")?.Value
        ?? "-";

    // GET: /Person — لیست اشخاص + پنل آموزشی JWT (+ حالت ویرایش با ?editId=n)
    [HttpGet]
    public async Task<IActionResult> Index(int? editId)
        => View(await BuildIndexViewModelAsync(editId));

    // POST: /Person/Add — افزودن شخص جدید
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add([Bind("FirstName,LastName,Age")] Person person)
    {
        if (!ModelState.IsValid)
            return View("Index", await BuildIndexViewModelAsync(null, person));

        await _unitOfWork.Persons.AddAsync(new Person
        {
            FirstName = person.FirstName.Trim(),
            LastName = person.LastName.Trim(),
            Age = person.Age,
        });
        await _unitOfWork.SaveChangesAsync();

        return RedirectToAction(nameof(Index)); // الگوی Post/Redirect/Get
    }

    // POST: /Person/Edit — ویرایش شخص
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([Bind("Id,FirstName,LastName,Age")] Person person)
    {
        var existing = await _unitOfWork.Persons.GetByIdAsync(person.Id);
        if (existing is null)
            return RedirectToAction(nameof(Index));

        if (!ModelState.IsValid)
            return View("Index", await BuildIndexViewModelAsync(person.Id, person));

        existing.FirstName = person.FirstName.Trim();
        existing.LastName = person.LastName.Trim();
        existing.Age = person.Age;
        _unitOfWork.Persons.Update(existing);
        await _unitOfWork.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // POST: /Person/Delete — حذف شخص
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var person = await _unitOfWork.Persons.GetByIdAsync(id);
        if (person is not null)
        {
            _unitOfWork.Persons.Remove(person);
            await _unitOfWork.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: /Person/RefreshToken — دکمه‌ی «صدور توکن جدید» در پنل آموزشی؛
    // اعتبارسنجی و چرخش توکن‌ها در گام‌های 19 و 20 (AuthService) انجام می‌شود
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RefreshToken()
    {
        await _authService.TryRefreshAsync(HttpContext);
        return RedirectToAction(nameof(Index));
    }

    // ساخت مدل نمایش صفحه: جدول + فرم‌ها + داده‌های پنل آموزشی JWT
    private async Task<PersonIndexViewModel> BuildIndexViewModelAsync(int? editId, Person? formPerson = null)
    {
        var people = await _unitOfWork.Persons.GetAllAsync();
        var editing = editId is null ? null : people.FirstOrDefault(p => p.Id == editId);

        var model = new PersonIndexViewModel
        {
            People = people,
            EditId = editId,
            Person = formPerson ?? editing ?? new Person(),
            CurrentUsername = CurrentUsername,
        };

        // اگر در همین درخواست «تمدید خودکار» انجام شده باشد (گام 21)، توکن‌های جدید
        // در HttpContext.Items هستند؛ در غیر این صورت از کوکی می‌خوانیم.
        var accessToken = HttpContext.Items["CurrentAccessToken"] as string
                          ?? Request.Cookies[AuthService.AccessTokenCookie];

        if (!string.IsNullOrEmpty(accessToken))
        {
            model.AccessToken = accessToken;
            try
            {
                // فقط پارس توکن برای نمایش — اعتبارسنجی واقعی قبلاً توسط JwtBearer انجام شده
                var jwt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);

                // گام 14: Claims خام داخل توکن + زمان انقضا برای نمایش آموزشی
                model.TokenClaims = jwt.Claims.ToList();
                model.AccessExpiresAtUtc = jwt.ValidTo;
                model.TokenHeaderJson = JsonSerializer.Serialize(jwt.Header, new JsonSerializerOptions { WriteIndented = true });
                model.TokenPayloadJson = JsonSerializer.Serialize(jwt.Payload, new JsonSerializerOptions { WriteIndented = true });
            }
            catch
            {
                model.AccessToken = null;
            }
        }

        var rawRefreshToken = HttpContext.Items["CurrentRefreshToken"] as string
                              ?? Request.Cookies[AuthService.RefreshTokenCookie];

        if (!string.IsNullOrEmpty(rawRefreshToken))
        {
            // نمایش ماسک‌شده — مقدار کامل توکن تمدید نباید فاش شود
            model.RefreshTokenMasked = rawRefreshToken.Length <= 16
                ? rawRefreshToken
                : $"{rawRefreshToken[..10]}…{rawRefreshToken[^6..]}";

            // زمان انقضای توکن تمدید از دیتابیس خوانده می‌شود (گام 17)
            var stored = await _unitOfWork.RefreshTokens
                .GetByTokenHashAsync(_tokenService.HashToken(rawRefreshToken));
            model.RefreshExpiresAtUtc = stored?.ExpiresAtUtc;
        }

        return model;
    }
}
