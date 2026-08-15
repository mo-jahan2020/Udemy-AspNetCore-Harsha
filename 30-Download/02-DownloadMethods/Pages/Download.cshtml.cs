// ====================================================================
// Download.cshtml.cs - کدBehind صفحه Razor Page
// ====================================================================
// Razor Page ها در ASP.NET Core یک روش ساده‌تر نسبت به MVC هستند.
// هر صفحه از دو فایل تشکیل شده:
// 1. Download.cshtml.cs - کد C# (مدل صفحه)
// 2. Download.cshtml - HTML و Razor
//
// در Razor Page ها به جای اکشن متد از Handler متد استفاده می‌شود.
// الگوی نام‌گذاری: OnGet، OnPost، OnGetDownload و غیره
// ====================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DownloadMethods.Pages
{
    public class DownloadModel : PageModel
    {
        // ====================================================================
        // تزریق وابستگی
        // ====================================================================
        // IWebHostEnvironment برای دسترسی به مسیر wwwroot
        // در Razor Page ها هم DI به همان شکل MVC کار می‌کند
        private readonly IWebHostEnvironment _env;

        // کنستراکتور - DI به صورت خودکار انجام می‌شود
        public DownloadModel(IWebHostEnvironment env)
        {
            _env = env;
        }

        // ====================================================================
        // روش ۱۱: دانلود فایل با Razor Page Handler
        // ====================================================================
        // Handler متدها در Razor Page جایگزین اکشن متدهای کنترلر هستند.
        // نام OnGetDownload به این معنی است:
        // - OnGet: متد HTTP GET است
        // - Download: نام Handler (در URL با ?handler=Download فراخوانی می‌شود)
        //
        // نحوه فراخوانی در مرورگر:
        //   /Download?handler=Download
        //   یا (با Route):
        //   /Download/Download
        //
        // مزیت Razor Page نسبت به MVC:
        // - ساده‌تر و متمرکزتر (کد و ویو در یک پوشه)
        // - برای صفحات مستقل مناسب‌تر است
        // ====================================================================
        public IActionResult OnGetDownload()
        {
            // ساختن مسیر کامل فایل
            var filePath = Path.Combine(_env.WebRootPath, "sample.pdf");

            // بررسی وجود فایل
            if (!System.IO.File.Exists(filePath))
                return NotFound("فایل مورد نظر یافت نشد!");

            // خواندن بایت‌های فایل
            var fileBytes = System.IO.File.ReadAllBytes(filePath);

            // برگرداندن فایل با نام سفارشی
            // در Razor Page هم از File() همان MVC استفاده می‌شود
            return File(fileBytes, "application/pdf", "razor-page-download.pdf");
        }
    }
}