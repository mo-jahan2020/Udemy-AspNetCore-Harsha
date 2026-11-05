using System.Net;
using JwtDemoMVC.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace JwtDemoMVC.Middleware;

// ═══════════════════════════════════════════════════════════════════════════
//  IpAllowListMiddleware — ویژگی IP AllowList (نسخه‌ی Web API خالص)
//  فقط IPهایی که در جدول AllowedIps (دیتابیس) فعال ثبت شده باشند اجازه دارند
//  از APIهای محافظت‌شده (persons / debug / settings) استفاده کنند.
//
//  چرا Middleware؟ محدودسازی IP یک «نگرانی سطح‌عرض» (Cross-cutting) است:
//    • کل ناحیه‌ی محافظت‌شده را با یک منطق واحد پوشش می‌دهد
//    • قبل از احراز هویت اجرا می‌شود و درخواست IP نامجاز را همان‌جا قطع می‌کند (Fail-fast)
//    • کنترلرها و [Authorize] هیچ خبری از آن ندارند و منطقشان تمیز می‌ماند
//  (جایگزین: Authorization Policy سفارشی — برای محدودسازی «بر هر اکشن مشخص» بهتر است،
//   ولی برای «کل ناحیه‌ی محافظت‌شده» Middleware ساده‌تر و خواناتر است.)
//
//  ترتیب در خط لوله (Program.cs):
//    Swagger (عمومی) → این Middleware → UseAuthentication → UseAuthorization
//  یعنی: اول «کامپیوترِ فراخوان» بررسی می‌شود، بعد «توکن کاربر».
//
//  مسیرهای مستثنی: api/auth، Swagger — (این‌ها «استفاده از JWT» نیستند و باید
//  برای گرفتن اولین توکن در دسترس باشند). مدیریت فهرست مجازها با api/settings.
//  در نسخه‌ی Web API دیگر ریدایرکت یا صفحه‌ی «دسترسی رد شد» وجود ندارد؛
//  همیشه پاسخ استاندارد 403 JSON برگردانده می‌شود.
// ═══════════════════════════════════════════════════════════════════════════
public class IpAllowListMiddleware
{
    // مسیرهایی که «استفاده از JWT» حساب می‌شوند و IPشان بررسی می‌شود.
    // (در نسخه‌ی MVC، پیشوندهای صفحات /Person و /Settings هم اینجا بودند — حذف شدند)
    // برای تغییر دامنه‌ی محدودسازی فقط همین آرایه را ویرایش کنید.
    public static readonly string[] ProtectedPrefixes =
        ["/api/persons", "/api/debug", "/api/settings"];

    // کلید کش در حافظه — پس از هر تغییر در api/settings، با IMemoryCache.Remove باطل می‌شود
    public const string CacheKey = "allowed-ips";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(30);

    private readonly RequestDelegate _next;

    public IpAllowListMiddleware(RequestDelegate next) => _next = next;

    // سرویس‌های Scoped (Repository) به‌صورت پارامتر داخل InvokeAsync تزریق می‌شوند —
    // یعنی برای «هر درخواست» یک نمونه‌ی تازه، دقیقاً مثل کنترلرها.
    public async Task InvokeAsync(HttpContext context, IAllowedIpRepository allowedIps, IMemoryCache cache)
    {
        var path = context.Request.Path.Value ?? "/";

        if (IsProtected(path))
        {
            // IP واقعی اتصال TCP — نه هر چیزی که کلاینت ادعا می‌کند.
            // در Production پشت Reverse Proxy، باید ForwardedHeaders فعال شود تا X-Forwarded-For معتبر خوانده شود.
            var remoteIp = NormalizeIp(context.Connection.RemoteIpAddress);

            // لیست IPهای مجاز از دیتابیس می‌آید ولی با کش ۳۰ ثانیه‌ای — تا هر درخواست به دیتابیس نخورد.
            // با هر Create/Delete/Update در api/settings، کش باطل می‌شود (SettingsController.InvalidateCache).
            // («?? []» فقط برای خوش‌رفتاری با nullable است؛ فکتوری همیشه لیست غیرتهی برمی‌گرداند)
            var allowed = await cache.GetOrCreateAsync(CacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;
                return allowedIps.GetActiveIpsAsync();
            }) ?? [];

            var match = allowed.FirstOrDefault(x =>
                string.Equals(x.IpAddress, remoteIp, StringComparison.OrdinalIgnoreCase));

            if (match is null)
            {
                // در Web API → پاسخ استاندارد JSON با کد 403 + نمایش IP فراخوان
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new
                {
                    message = "IP شما در فهرست IPهای مجاز برای استفاده از این API نیست یا اعتبارش تمام شده است.",
                    yourIp = remoteIp,
                });
                return; // زنجیره‌ی Middleware را قطع کن — درخواست به [Authorize] و کنترلر نمی‌رسد
            }
        }

        await _next(context);
    }

    private static bool IsProtected(string path)
        => ProtectedPrefixes.Any(p => Matches(path, p));

    // تطابق «دقیق یا زیرمسیر»: /api/persons با /api/persons/5 می‌خواند اما با /api/personsFoo نه
    private static bool Matches(string path, string prefix)
        => path.Equals(prefix, StringComparison.OrdinalIgnoreCase)
           || path.StartsWith(prefix + "/", StringComparison.OrdinalIgnoreCase);

    // نرمال‌سازی برای مقایسه‌ی یکدست: IPv6-mapped IPv4 (::ffff:1.2.3.4) → IPv4 خام (1.2.3.4)
    public static string NormalizeIp(IPAddress? ip)
    {
        if (ip is null) return "-"; // بدون IP (نادر) → با هیچ رکوردی match نمی‌شود → رد می‌شود
        if (ip.IsIPv4MappedToIPv6) ip = ip.MapToIPv4();
        return ip.ToString();
    }

    // پارس و نرمال‌سازی ورودی کاربر در api/settings — همان نرمال‌سازیِ Middleware،
    // تا چیزی که ذخیره می‌شود دقیقاً با چیزی که هنگام مقایسه ساخته می‌شود یکی باشد.
    public static bool TryNormalize(string? input, out string normalized)
    {
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(input))
            return false;

        var trimmed = input.Trim();

        // IPAddress.TryParse فرم‌های کوتاه IPv4 مثل «1.2» یا «1.2.3» را هم می‌پذیرد؛
        // برای دقت، IPv4 فقط با هر 4 بخش پذیرفته می‌شود.
        if (!trimmed.Contains(':') && trimmed.Split('.').Length != 4)
            return false;

        if (!IPAddress.TryParse(trimmed, out var ip))
            return false;

        if (ip.IsIPv4MappedToIPv6)
            ip = ip.MapToIPv4();

        normalized = ip.ToString();
        return true;
    }
}
