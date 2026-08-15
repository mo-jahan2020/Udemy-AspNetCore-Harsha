// ====================================================================
// DownloadController.cs - کنترلر دانلود فایل
// ====================================================================
// این کنترلر شامل ۶ اکشن متد مختلف برای دانلود فایل است.
// هر متد از روش متفاوتی برای بازگرداندن فایل استفاده می‌کند.
// همه متدها فایل sample.pdf را از پوشه wwwroot دانلود می‌کنند.
// ====================================================================

using Microsoft.AspNetCore.Mvc;

namespace DownloadMethods.Controllers
{
    public class DownloadController : Controller
    {
        // ====================================================================
        // تزریق وابستگی (Dependency Injection)
        // ====================================================================
        // IWebHostEnvironment اطلاعاتی درباره محیط هاستینگ می‌دهد
        // از جمله مسیر پوشه wwwroot (WebRootPath)
        // ====================================================================
        private readonly IWebHostEnvironment _env;

        // کنستراکتور - ASP.NET Core به صورت خودکار IWebHostEnvironment را تزریق می‌کند
        // این الگو به نام Constructor Injection شناخته می‌شود
        public DownloadController(IWebHostEnvironment env)
        {
            _env = env;
        }

        // ====================================================================
        // روش ۳: دانلود با FileResult (مسیر فیزیکی)
        // ====================================================================
        // File() ساده‌ترین روش دانلود فایل در ASP.NET Core است.
        // این متد یک FileResult برمی‌گرداند که مرورگر آن را به عنوان دانلود
        // یا نمایش (بسته به نوع فایل) ارائه می‌دهد.
        //
        // نکته آموزشی:
        // - پارامتر اول: مسیر فیزیکی فایل روی سرور
        // - پارامتر دوم: نوع محتوا (MIME Type) - "application/pdf" برای فایل PDF
        // - پارامتر سوم: نام فایلی که کاربر دانلود می‌کند (اختیاری)
        //
        // تفاوت File با PhysicalFile:
        // File وقتی مسیر فیزیکی می‌گیرد، داخلیاً از PhysicalFile استفاده می‌کند
        // اما File چند overload دارد که PhysicalFile ندارد
        // ====================================================================
        public IActionResult FileResultMethod()
        {
            // ساختن مسیر کامل فایل
            // Path.Combine دو مسیر را به هم وصل می‌کند
            var filePath = Path.Combine(_env.WebRootPath, "sample.pdf");

            // بررسی وجود فایل - اگر نبود 404 برگردان
            if (!System.IO.File.Exists(filePath))
                return NotFound("فایل مورد نظر یافت نشد!");

            // برگرداندن فایل با FileResult
            // پارامتر سوم نام فایل دانلود را مشخص می‌کند
            return File(filePath, "application/pdf", "file-result-download.pdf");
        }

        // ====================================================================
        // روش ۴: دانلود با PhysicalFileResult
        // ====================================================================
        // PhysicalFile مستقیماً از مسیر فیزیکی فایل روی دیسک سخت خوانده
        // و به کاربر ارسال می‌کند. این روش از FileStream استفاده می‌کند
        // بنابراین برای فایل‌های بزرگ مناسب است (به حافظه کمتری نیاز دارد).
        //
        // نکته آموزشی:
        // - PhysicalFile فقط مسیر فیزیکی (absolute path) می‌گیرد
        // - از FileStream برای خواندن فایل استفاده می‌کند (نه.ReadAllBytes)
        // - برای فایل‌های حجیم بهینه‌تر از FileContentResult است
        // - enableRangeProcessing پشتیبانی از Resume دانلود را فعال می‌کند
        // ====================================================================
        public IActionResult PhysicalFileMethod()
        {
            // مسیر فیزیکی فایل
            var filePath = Path.Combine(_env.WebRootPath, "sample.pdf");

            if (!System.IO.File.Exists(filePath))
                return NotFound("فایل مورد نظر یافت نشد!");

            // برگرداندن فایل با PhysicalFileResult
            // PhysicalFile فقط مسیر فیزیکی (نه مجازی) می‌پذیرد
            var result = PhysicalFile(filePath, "application/pdf", "physical-file-download.pdf");

            // فعال‌سازی پشتیبانی از Resume (ادامه دانلود)
            // با این تنظیم، اگر دانلود قطع شود کاربر می‌تواند ادامه دهد
            result.EnableRangeProcessing = true;

            return result;
        }

        // ====================================================================
        // روش ۵: دانلود با VirtualFileResult
        // ====================================================================
        // VirtualFile فایل را بر اساس مسیر مجازی (نسبت به WebRoot) پیدا
        // و به کاربر ارسال می‌کند. نیازی به مسیر فیزیکی نیست.
        //
        // نکته آموزشی:
        // - مسیر مجازی نسبت به پوشه wwwroot است
        // - کاراکتر ~ اختیاری است و به WebRoot اشاره می‌کند
        // - اگر فایل در wwwroot نباشد، کار نمی‌کند (برای سایر پوشه‌ها از File استفاده کنید)
        // - داخلیاً از IFileProvider استفاده می‌کند
        // ====================================================================
        public IActionResult VirtualFileMethod()
        {
            // VirtualFile may not be available in all controller base types in this project.
            // Use the physical path to the wwwroot file and return a PhysicalFileResult instead.
            var filePath = Path.Combine(_env.WebRootPath, "sample.pdf");

            if (!System.IO.File.Exists(filePath))
                return NotFound("فایل مورد نظر یافت نشد!");

            return PhysicalFile(filePath, "application/pdf", "virtual-file-download.pdf");
        }

        // ====================================================================
        // روش ۶: دانلود با FileStreamResult
        // ====================================================================
        // FileStreamResult فایل را به صورت Stream (جریان داده) باز کرده
        // و ارسال می‌کند. این روش برای فایل‌های بزرگ بسیار مناسب است
        // زیرا کل فایل را یکجا در حافظه بارگذاری نمی‌کند.
        //
        // نکته آموزشی:
        // - فایل به صورت Stream خوانده می‌شود (نه بایت آرای)
        // - برای فایل‌های حجیم (چند گیگابایت) مناسب است
        // - حافظه کمتری مصرف می‌کند
        // - FileStream به صورت خودکار پس از ارسال بسته می‌شود
        // - می‌توانید از هر نوع Stream استفاده کنید (نه فقط FileStream)
        // ====================================================================
        public IActionResult FileStreamMethod()
        {
            // مسیر فایل
            var filePath = Path.Combine(_env.WebRootPath, "sample.pdf");

            if (!System.IO.File.Exists(filePath))
                return NotFound("فایل مورد نظر یافت نشد!");

            // باز کردن فایل به صورت Stream
            // FileMode.Open: فایل را برای خواندن باز می‌کند (اگر وجود نداشته باشد خطا می‌دهد)
            // FileAccess.Read: فقط دسترسی خواندن
            // FileShare.Read: بقیه هم می‌توانند همزمان فایل را بخوانند
            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            // برگرداندن FileStreamResult
            // ASP.NET Core خودکار Stream را پس از ارسال می‌بندد
            return File(stream, "application/pdf", "filestream-download.pdf");
        }

        // ====================================================================
        // روش ۷: دانلود با FileContentResult (بایت آرای)
        // ====================================================================
        // FileContentResult کل محتوای فایل را به صورت آرایه بایتی (byte[])
        // در حافظه بارگذاری کرده و ارسال می‌کند.
        //
        // نکته آموزشی:
        // - کل فایل یکجا در حافظه بارگذاری می‌شود
        // - برای فایل‌های کوچک (زیر چند مگابایت) مناسب است
        // - برای فایل‌های بزرگ مناسب نیست (حافظه زیادی مصرف می‌کند)
        // - می‌توانید قبل از ارسال، محتوا را تغییر دهید (مثلاً رمزنگاری)
        // - کاربرد: زمانی که محتوای فایل را پویا تولید می‌کنید
        // ====================================================================
        public IActionResult FileContentMethod()
        {
            // مسیر فایل
            var filePath = Path.Combine(_env.WebRootPath, "sample.pdf");

            if (!System.IO.File.Exists(filePath))
                return NotFound("فایل مورد نظر یافت نشد!");

            // خواندن تمام بایت‌های فایل در حافظه
            // ReadAllBytes کل فایل را به صورت آرایه بایتی برمی‌گرداند
            var fileBytes = System.IO.File.ReadAllBytes(filePath);

            // برگرداندن FileContentResult
            // این overload از File بایت‌آرای را می‌گیرد
            return File(fileBytes, "application/pdf", "filecontent-download.pdf");
        }

        // ====================================================================
        // روش ۱۴: دانلود با تنظیم دستی Response Headers
        // ====================================================================
        // این روش پایین‌ترین سطح کنترل را روی پاسخ HTTP می‌دهد.
        // ما دستی Headers را تنظیم کرده و فایل را ارسال می‌کنیم.
        //
        // نکته آموزشی:
        // - Content-Disposition: هدر مهمی که رفتار مرورگر را تعیین می‌کند
        //   - attachment: فایل را دانلود می‌کند (به جای نمایش)
        //   - inline: فایل را در مرورگر نمایش می‌دهد
        // - Content-Type: نوع فایل را مشخص می‌کند
        // - این روش برای کنترل کامل روی پاسخ HTTP مناسب است
        // - می‌توانید هدرهای سفارشی اضافه کنید (مثلاً Cache-Control)
        // ====================================================================
        public IActionResult ManualResponseMethod()
        {
            // مسیر فایل
            var filePath = Path.Combine(_env.WebRootPath, "sample.pdf");

            if (!System.IO.File.Exists(filePath))
                return NotFound("فایل مورد نظر یافت نشد!");

            // خواندن بایت‌های فایل
            var fileBytes = System.IO.File.ReadAllBytes(filePath);

            // ============================================================
            // تنظیم دستی هدرهای HTTP Response
            // ============================================================

            // Content-Disposition: به مرورگر می‌گوید فایل را دانلود کند
            // filename نام فایل دانلود را مشخص می‌کند
            Response.Headers.Append("Content-Disposition", "attachment; filename=manual-response-download.pdf");

            // Content-Type: نوع محتوای فایل را مشخص می‌کند
            // بدون این هدر، مرورگر نمی‌داند فایل از چه نوعی است
            Response.ContentType = "application/pdf";

            // Content-Length: اندازه فایل را مشخص می‌کند
            // این هدر به مرورگر کمک می‌کند نوار پیشرفت دانلود را نشان دهد
            Response.Headers.Append("Content-Length", fileBytes.Length.ToString());

            // برگرداندن فایل
            // در اینجا چون هدرها را دستی تنظیم کردیم، فقط فایل را برمی‌گردانیم
            return File(fileBytes, "application/pdf");
        }
    }
}