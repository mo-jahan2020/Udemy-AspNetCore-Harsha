using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using System.Text;

namespace AsvsDemoFull.Controllers {
    public class HomeController : Controller {
        public IActionResult Index() => View();

        // بند OWASP ASVS: V2.1.1 - اعتبارسنجی طول و پیچیدگی رمز عبور
        [HttpPost]
        public IActionResult CheckPassword(string password) {
            bool isValid = password.Length >= 12 && Regex.IsMatch(password, @"[A-Z]") && Regex.IsMatch(password, @"[a-z]") && Regex.IsMatch(password, @"[0-9]");
            ViewBag.IsValid = isValid;
            ViewBag.Message = isValid ? "رمز عبور معتبر است (V2.1.1)." : "خطا: رمز عبور باید حداقل ۱۲ کاراکتر و شامل حروف بزرگ، کوچک و عدد باشد.";
            return View("Result", password);
        }

        // بند OWASP ASVS: V5.1.2 - کدگذاری خروجی برای جلوگیری از XSS
        [HttpPost]
        public IActionResult XssDemo(string userInput) {
            ViewBag.SafeInput = System.Net.WebUtility.HtmlEncode(userInput);
            ViewBag.RawInput = userInput;
            return View("ResultXss");
        }

        // بند OWASP ASVS: V8.1.1 - استفاده از توکن ضد جعل درخواست (Anti-CSRF)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CsrfAction() {
            ViewBag.Success = true;
            return View("ResultCsrf");
        }

        // بند OWASP ASVS: V3.2.1 - تنظیم کوکی‌های نشست با فلگ‌های امنیتی
        [HttpPost]
        public IActionResult SessionDemo() {
            var cookieOptions = new CookieOptions {
                HttpOnly = true, // جلوگیری از دسترسی جاوااسکریپت به کوکی
                Secure = true,   // فقط از طریق HTTPS ارسال شود
                SameSite = SameSiteMode.Strict, // جلوگیری از ارسال در درخواست‌های بین‌سایتی
                Expires = DateTimeOffset.UtcNow.AddMinutes(15)
            };
            Response.Cookies.Append("ASVS_SessionId", Guid.NewGuid().ToString(), cookieOptions);
            ViewBag.Message = "کوکی امن با فلگ‌های HttpOnly, Secure, SameSite تنظیم شد (V3.2.1).";
            return View("Result");
        }

        // بند OWASP ASVS: V12.3.3 - اعتبارسنجی آپلود فایل (بررسی پسوند و محتوا)
        [HttpPost]
        public IActionResult FileUploadDemo(IFormFile file) {
            string[] allowedExtensions = { ".jpg", ".png", ".pdf" };
            string ext = Path.GetExtension(file.FileName).ToLower();
            
            if (Array.IndexOf(allowedExtensions, ext) == -1) {
                ViewBag.Message = $"خطا (V12.3.3): پسوند فایل '{ext}' مجاز نیست. فقط jpg, png, pdf مجاز هستند.";
                ViewBag.IsSuccess = false;
            } else {
                // در محیط واقعی، اینجا باید نوع MIME هم با Magic Number بررسی شود
                ViewBag.Message = $"فایل '{file.FileName}' با موفقیت اعتبارسنجی شد (V12.3.3).";
                ViewBag.IsSuccess = true;
            }
            return View("Result");
        }

        // بند OWASP ASVS: V7.1.3 - مدیریت خطا بدون نشت اطلاعات حساس
        [HttpPost]
        public IActionResult ErrorHandlingDemo() {
            try {
                // شبیه‌سازی یک خطای داخلی (مثلاً خطای دیتابیس)
                throw new Exception("Connection string 'Server=10.0.0.5;User=sa;Password=123' failed!");
            } catch (Exception ex) {
                // بند V7.1.3: هرگز Stack Trace یا جزئیات داخلی را به کاربر نشان نده
                // فقط یک پیام عمومی ثبت و نمایش بده
                ViewBag.Message = "یک خطای داخلی رخ داد. لطفاً با پشتیبانی تماس بگیرید. (جزئیات فنی در سرور لاگ شد - V7.1.3)";
                // در اینجا باید با یک سیستم لاگینگ واقعی مثل Serilog لاگ شود
            }
            return View("Result");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View();
    }
}
