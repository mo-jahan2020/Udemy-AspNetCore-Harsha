using Microsoft.AspNetCore.Mvc;

namespace AsvsIntegratedDemo.Controllers {
    public class InputValidationController : Controller {
        public IActionResult Index() => View();

        // بند OWASP ASVS: V5.1.2 - کدگذاری خروجی برای جلوگیری از XSS
        [HttpPost]
        public IActionResult XssDemo(string userInput) {
            ViewBag.SafeInput = System.Net.WebUtility.HtmlEncode(userInput);
            ViewBag.RawInput = userInput;
            return View("Result");
        }
    }
}
