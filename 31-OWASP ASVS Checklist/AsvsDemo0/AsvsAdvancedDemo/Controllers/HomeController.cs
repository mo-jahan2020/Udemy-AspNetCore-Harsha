using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace AsvsDemo.Controllers {
    public class HomeController : Controller {
        public IActionResult Index() => View();

        // بند OWASP ASVS: V2.1.1 - اعتبارسنجی طول و پیچیدگی رمز عبور
        [HttpPost]
        public IActionResult CheckPassword(string password) {
            bool isValid = true;
            string message = "رمز عبور معتبر است و با استاندارد ASVS مطابقت دارد.";
            if (string.IsNullOrEmpty(password) || password.Length < 12) { isValid = false; message = "خطا: رمز عبور باید حداقل ۱۲ کاراکتر باشد."; }
            else if (!Regex.IsMatch(password, @"[A-Z]")) { isValid = false; message = "خطا: رمز عبور باید حداقل یک حرف بزرگ انگلیسی داشته باشد."; }
            else if (!Regex.IsMatch(password, @"[a-z]")) { isValid = false; message = "خطا: رمز عبور باید حداقل یک حرف کوچک انگلیسی داشته باشد."; }
            else if (!Regex.IsMatch(password, @"[0-9]")) { isValid = false; message = "خطا: رمز عبور باید حداقل یک عدد داشته باشد."; }
            ViewBag.IsValid = isValid;
            ViewBag.Message = message;
            return View("PasswordDemo", password);
        }

        // بند OWASP ASVS: V5.1.2 - کدگذاری خروجی (Output Encoding) برای جلوگیری از XSS
        [HttpPost]
        public IActionResult XssDemo(string userInput) {
            ViewBag.SafeInput = System.Net.WebUtility.HtmlEncode(userInput);
            ViewBag.RawInput = userInput;
            return View();
        }

        // بند OWASP ASVS: V8.1.1 - استفاده از توکن ضد جعل درخواست (Anti-CSRF)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CsrfAction() {
            ViewBag.Success = true;
            return View("CsrfDemo");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View();
    }
}
