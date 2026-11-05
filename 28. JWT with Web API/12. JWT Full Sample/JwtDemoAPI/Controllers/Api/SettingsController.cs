using JwtDemoMVC.Middleware;
using JwtDemoMVC.Models;
using JwtDemoMVC.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace JwtDemoMVC.Controllers.Api
{
    /// <summary>
    /// API مدیریت IPهای مجاز (ویژگی IP AllowList) — قابل تست در Swagger.
    /// همان داده‌ای را مدیریت می‌کند که صفحه‌ی MVC «/Settings» مدیریت می‌کند.
    ///
    /// نکته‌های طراحی:
    ///  • ورودی/خروجی DTO هستند، نه Entity → جلوگیری از Over-posting و افشای ستون‌های داخلی.
    ///  • IP با همان <see cref="IpAllowListMiddleware.TryNormalize"/> نرمال‌سازی می‌شود
    ///    که Middleware هنگام مقایسه استفاده می‌کند → ذخیره و مقایسه دقیقاً یکسان‌اند.
    ///  • بعد از هر تغییر، کش Middleware باطل می‌شود تا اثر فوری باشد (نه تا ۳۰ ثانیه بعد).
    ///  • ثبت تکراری (هم چک قبلی و هم Race Condition ایندکس یونیک) → 409 به‌جای 500.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [Authorize]
    public class SettingsController : ControllerBase
    {
        // پیش‌فرض Create: یک سال (حفظ رفتار نسخه‌ی قبلی) — سقف: ده سال، هم‌راستا با ViewModel
        private const int DefaultCreateValidityMinutes = 1_440;// 1 day
        private const int MaxValidityMinutes = 5_256_000;// 10 year

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;
        private readonly ILogger<SettingsController> _logger;

        public SettingsController(
            IUnitOfWork unitOfWork,
            IMemoryCache cache,
            ILogger<SettingsController> logger)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>لیست همه‌ی IPهای مجاز (نزدیک‌ترین انقضا اول)</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<AllowedIpResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAll()
        {
            var items = await _unitOfWork.AllowedIps.GetAllSortedAsync();
            return Ok(items.Select(AllowedIpResponse.FromEntity).ToList());
        }

        /// <summary>دریافت یک IP مجاز با شناسه</summary>
        /// <param name="id">شناسه‌ی رکورد IP</param>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(AllowedIpResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetById(int id)
        {
            if (id < 1)
                return NotFound(new { message = "IP با این شناسه یافت نشد." });

            var entity = await _unitOfWork.AllowedIps.GetByIdAsync(id);
            return entity is null
                ? NotFound(new { message = "IP با این شناسه یافت نشد." })
                : Ok(AllowedIpResponse.FromEntity(entity));
        }

        /// <summary>افزودن IP مجاز</summary>
        /// <remarks>
        /// ValidityMinutes اختیاری است؛ اگر ارسال نشود یک روز در نظر گرفته می‌شود.
        /// حداکثر ۱۰ سال (5,256,000 دقیقه).
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(typeof(AllowedIpResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create([FromBody] AllowedIpRequest request)
        {
            // نرمال‌سازی + اعتبارسنجی آدرس IP (IPv4 باید ۴ بخش داشته باشد؛ IPv6 پذیرفته می‌شود)
            if (!IpAllowListMiddleware.TryNormalize(request.IpAddress, out var normalized))
            {
                return BadRequest(new
                {
                    message = "آدرس IP معتبر نیست (مثال: 192.168.1.10 یا ::1)",
                    field = nameof(request.IpAddress),
                });
            }

            // چک تکراری بودن پیش از Insert (پیام دوستانه در حالت عادی)
            if (await _unitOfWork.AllowedIps.GetByIpAsync(normalized) is not null)
                return Conflict(new { message = "این IP قبلاً ثبت شده است." });

            var minutes = request.ValidityMinutes ?? DefaultCreateValidityMinutes;
            if (minutes is < 1 or > MaxValidityMinutes)
                return BadRequest(new { message = $"مدت اعتبار باید بین ۱ تا {MaxValidityMinutes:N0} دقیقه باشد." });

            // یک بار UtcNow را می‌گیریم تا CreatedAt و ExpiresAt دقیقاً از یک لحظه محاسبه شوند
            var now = DateTime.UtcNow;
            var entity = new AllowedIp
            {
                IpAddress = normalized,
                Note = NormalizeNote(request.Note),
                CreatedAtUtc = now,
                ExpiresAtUtc = now.AddMinutes(minutes),
            };

            try
            {
                await _unitOfWork.AllowedIps.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();
            }
            // Race Condition: بین چک تکراری و Insert، درخواست هم‌زمان دیگری همان IP را ثبت کرده
            // → ایندکس یونیک خطای 2601/2627 می‌دهد. به‌جای 500، همان 409 دوستانه را برگردان.
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                _logger.LogWarning(ex, "ثبت تکراری IP مجاز به دلیل درخواست هم‌زمان: {Ip}", normalized);
                return Conflict(new { message = "این IP قبلاً ثبت شده است." });
            }

            InvalidateCache();
            _logger.LogInformation("IP مجاز جدید ثبت شد: {Ip} با اعتبار {Minutes} دقیقه", normalized, minutes);
            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, AllowedIpResponse.FromEntity(entity));
        }

        /// <summary>ویرایش IP مجاز</summary>
        /// <remarks>
        /// CreatedAtUtc هرگز تغییر نمی‌کند. اگر ValidityMinutes ارسال شود، تاریخ انقضا از «اکنون»
        /// بازمحاسبه می‌شود؛ اگر ارسال نشود، تاریخ انقضای فعلی دست‌نخورده می‌ماند.
        /// </remarks>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(AllowedIpResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Update(int id, [FromBody] AllowedIpRequest request)
        {
            if (id < 1)
                return NotFound(new { message = "IP با این شناسه یافت نشد." });

            // GetByIdAsync از FindAsync استفاده می‌کند → Entity tracked است و تغییراتش SaveChanges می‌شود
            var existing = await _unitOfWork.AllowedIps.GetByIdAsync(id);
            if (existing is null)
                return NotFound(new { message = "IP با این شناسه یافت نشد." });

            if (!IpAllowListMiddleware.TryNormalize(request.IpAddress, out var normalized))
            {
                return BadRequest(new
                {
                    message = "آدرس IP معتبر نیست (مثال: 192.168.1.10 یا ::1)",
                    field = nameof(request.IpAddress),
                });
            }

            // اگر IP عوض شده، نباید با رکورد دیگری تکراری شود
            if (!string.Equals(normalized, existing.IpAddress, StringComparison.OrdinalIgnoreCase)
                && await _unitOfWork.AllowedIps.GetByIpAsync(normalized) is not null)
            {
                return Conflict(new { message = "این IP قبلاً ثبت شده است." });
            }

            if (request.ValidityMinutes is int minutes && (minutes < 1 || minutes > MaxValidityMinutes))
                return BadRequest(new { message = $"مدت اعتبار باید بین ۱ تا {MaxValidityMinutes:N0} دقیقه باشد." });

            existing.IpAddress = normalized;
            existing.Note = NormalizeNote(request.Note);

            // باگ نسخه‌ی قبل: CreatedAtUtc اینجا با DateTime.UtcNow بازنویسی می‌شد.
            // تاریخ ایجاد باید ثابت بماند؛ فقط در صورت درخواست، انقضا تمدید می‌شود.
            if (request.ValidityMinutes is int validity)
                existing.ExpiresAtUtc = DateTime.UtcNow.AddMinutes(validity);

            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                _logger.LogWarning(ex, "ویرایش به IP تکراری منجر شد: {Ip}", normalized);
                return Conflict(new { message = "این IP قبلاً ثبت شده است." });
            }

            InvalidateCache();
            _logger.LogInformation("IP مجاز ویرایش شد: {Ip} (شناسه {Id})", normalized, id);
            return Ok(AllowedIpResponse.FromEntity(existing));
        }

        /// <summary>حذف IP مجاز</summary>
        /// <param name="id">شناسه‌ی رکورد IP</param>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Delete(int id)
        {
            // id نامعتبر → همان رفتار «چیزی برای حذف نیست» (Idempotent)، بدون رفتن به دیتابیس
            if (id < 1)
                return NoContent();

            var entity = await _unitOfWork.AllowedIps.GetByIdAsync(id);
            if (entity is null)
                return NotFound(new { message = "IP با این شناسه یافت نشد." });

            _unitOfWork.AllowedIps.Remove(entity);
            await _unitOfWork.SaveChangesAsync();

            InvalidateCache();
            _logger.LogInformation("IP مجاز حذف شد: {Ip} (شناسه {Id})", entity.IpAddress, id);
            return NoContent();
        }

        // ────────────────────────── Helpers ──────────────────────────

        // خطای نقض ایندکس یونیک در SQL Server: 2601 (duplicate key) و 2627 (unique constraint)
        private static bool IsUniqueViolation(DbUpdateException ex)
            => ex.InnerException is SqlException { Number: 2601 or 2627 };

        private static string? NormalizeNote(string? note)
            => string.IsNullOrWhiteSpace(note) ? null : note.Trim();

        // بعد از هر تغییر، کش Middleware بلافاصله باطل می‌شود تا تغییر بدون تأخیر اعمال شود
        private void InvalidateCache() => _cache.Remove(IpAllowListMiddleware.CacheKey);
    }
}