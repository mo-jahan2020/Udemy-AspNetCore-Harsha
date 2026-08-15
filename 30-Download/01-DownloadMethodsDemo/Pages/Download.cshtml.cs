// فضای نام‌های مورد نیاز
using Microsoft.AspNetCore.Mvc;                         // برای IActionResult ها
using Microsoft.AspNetCore.Mvc.RazorPages;              // برای PageModel
using Microsoft.AspNetCore.Hosting;                     // برای دسترسی به مسیر wwwroot
using System.IO;                                         // برای عملیات فایل

namespace DownloadMethodsDemo.Pages
{
    // مدل صفحه Razor Page مربوط به دانلود
    public class DownloadModel : PageModel
    {
        // فیلد محیط میزبانی برای گرفتن مسیر wwwroot
        private readonly IWebHostEnvironment _env;

        // سازنده برای تزریق IWebHostEnvironment
        public DownloadModel(IWebHostEnvironment env)
        {
            // ذخیره مقدار
            _env = env;
        }

        // متد کمکی جهت گرفتن مسیر فایل نمونه
        private string SamplePath => Path.Combine(_env.WebRootPath, "sample.pdf");

        // Handler پیش‌فرض GET – فقط صفحه را نمایش می‌دهد
        public void OnGet()
        {
            // بدنه خالی – فقط صفحه رندر می‌شود
        }

        // Handler سفارشی GET با نام Pdf – با آدرس ?handler=Pdf فراخوانی می‌شود
        public IActionResult OnGetPdf()
        {
            // خواندن بایت‌های فایل
            var bytes = System.IO.File.ReadAllBytes(SamplePath);
            // بازگشت فایل به کاربر
            return File(bytes, "application/pdf", "sample-razorpage-get.pdf");
        }

        // Handler POST پیش‌فرض – هنگام ارسال فرم اجرا می‌شود
        public IActionResult OnPost()
        {
            // استفاده از PhysicalFile برای ارسال بهینه فایل
            return PhysicalFile(SamplePath, "application/pdf", "sample-razorpage-post.pdf");
        }
    }
}
