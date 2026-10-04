using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace AsvsIntegratedDemo.Controllers {
    public class AuthenticationController : Controller {
        public IActionResult Index() => View();

        // بند OWASP ASVS: V2.1.1 - اعتبارسنجی طول و پیچیدگی رمز عبور
        [HttpPost]
        public IActionResult CheckPassword(string password) {
            bool isValid = password.Length >= 12 && 
                          Regex.IsMatch(password, "[A-Z]") && 
                          Regex.IsMatch(password, "[a-z]") && 
                          Regex.IsMatch(password, "[0-9]");
            ViewBag.IsValid = isValid;
            ViewBag.Message = isValid ? "رمز عبور معتبر است (V2.1.1)." : "خطا: رمز عبور باید حداقل ۱۲ کاراکتر و شامل حروف بزرگ، کوچک و عدد باشد.";
            return View("Result");
        }
    }
}
