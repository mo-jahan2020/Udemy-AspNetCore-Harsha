// فضای نام‌های مورد نیاز
using Microsoft.AspNetCore.Mvc;              // برای Controller و ActionResult ها
using Microsoft.AspNetCore.Hosting;          // برای IWebHostEnvironment جهت دسترسی به مسیر wwwroot
using System.IO;                              // برای کار با فایل‌سیستم

namespace DownloadMethodsDemo.Controllers
{
    // کنترلر اصلی برای نمایش صفحه و ارائه روش‌های مختلف دانلود
    public class HomeController : Controller
    {
        // فیلد نگهدارنده اطلاعات محیط میزبانی برای دسترسی به WebRootPath
        private readonly IWebHostEnvironment _env;

        // سازنده کنترلر که IWebHostEnvironment را از طریق DI دریافت می‌کند
        public HomeController(IWebHostEnvironment env)
        {
            // ذخیره محیط میزبانی در فیلد کلاس
            _env = env;
        }

        // متد کمکی برای گرفتن مسیر کامل فایل sample.pdf در پوشه wwwroot
        private string GetSamplePath()
        {
            // ترکیب مسیر wwwroot با نام فایل مورد نظر
            return Path.Combine(_env.WebRootPath, "sample.pdf");
        }

        // اکشن Index که ویو اصلی دارای دکمه‌های دانلود را برمی‌گرداند
        public IActionResult Index()
        {
            // برگرداندن View پیش‌فرض
            return View();
        }

        // -----------------------------------------------------------------
        // روش ۱: استفاده از File(byte[], contentType, fileName)
        // -----------------------------------------------------------------
        public IActionResult DownloadByteArray()
        {
            // خواندن تمام محتوای فایل به صورت آرایه بایت
            byte[] bytes = System.IO.File.ReadAllBytes(GetSamplePath());
            // برگرداندن FileContentResult با نوع محتوا application/pdf
            return File(bytes, "application/pdf", "sample-bytearray.pdf");
        }

        // -----------------------------------------------------------------
        // روش ۲: استفاده از File(Stream, contentType, fileName)
        // -----------------------------------------------------------------
        public IActionResult DownloadFileStream()
        {
            // باز کردن یک استریم از فایل برای خواندن
            var stream = new FileStream(GetSamplePath(), FileMode.Open, FileAccess.Read);
            // برگرداندن FileStreamResult
            return File(stream, "application/pdf", "sample-stream.pdf");
        }

        // -----------------------------------------------------------------
        // روش ۳: استفاده از PhysicalFile برای فایل فیزیکی روی دیسک
        // -----------------------------------------------------------------
        public IActionResult DownloadPhysicalFile()
        {
            // PhysicalFile نیاز به مسیر مطلق دارد و فایل را به صورت بهینه ارسال می‌کند
            return PhysicalFile(GetSamplePath(), "application/pdf", "sample-physical.pdf");
        }

        // -----------------------------------------------------------------
        // روش ۴: استفاده از VirtualFile با مسیر مجازی (نسبت به wwwroot)
        // -----------------------------------------------------------------
        public IActionResult DownloadVirtualFile()
        {
            // File با مسیر مجازی که ~ اشاره به ریشه wwwroot دارد
            return File("~/sample.pdf", "application/pdf", "sample-virtual.pdf");
        }

        // -----------------------------------------------------------------
        // روش ۵: استفاده از FileContentResult به صورت دستی
        // -----------------------------------------------------------------
        public IActionResult DownloadManualResult()
        {
            // خواندن بایت‌های فایل
            var bytes = System.IO.File.ReadAllBytes(GetSamplePath());
            // ساخت مستقیم FileContentResult
            var result = new FileContentResult(bytes, "application/pdf")
            {
                // تعیین نام فایل هنگام دانلود
                FileDownloadName = "sample-manual.pdf"
            };
            // بازگرداندن نتیجه
            return result;
        }

        // -----------------------------------------------------------------
        // روش ۶: نوشتن مستقیم روی Response.Body (روش سطح پایین)
        // -----------------------------------------------------------------
        public async Task DownloadRawResponse()
        {
            // تعیین نوع محتوای پاسخ به عنوان PDF
            Response.ContentType = "application/pdf";
            // افزودن هدر Content-Disposition برای وادار کردن مرورگر به دانلود
            Response.Headers.Append("Content-Disposition", "attachment; filename=sample-raw.pdf");
            // باز کردن استریم فایل و کپی آن روی بدنه پاسخ به صورت غیرهمزمان
            await using var fs = new FileStream(GetSamplePath(), FileMode.Open, FileAccess.Read);
            await fs.CopyToAsync(Response.Body);
        }

        // -----------------------------------------------------------------
        // روش ۷: تحویل داده به صورت JSON که در سمت کلاینت با JS دانلود می‌شود
        // -----------------------------------------------------------------
        public IActionResult GetFileAsBase64()
        {
            // خواندن بایت‌های فایل
            var bytes = System.IO.File.ReadAllBytes(GetSamplePath());
            // تبدیل به رشته Base64 برای ارسال درون JSON
            var base64 = System.Convert.ToBase64String(bytes);
            // برگرداندن JSON شامل نام فایل، نوع و محتوای Base64
            return Json(new { fileName = "sample-js.pdf", contentType = "application/pdf", data = base64 });
        }
    }
}
