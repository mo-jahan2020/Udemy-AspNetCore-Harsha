using Microsoft.AspNetCore.Mvc;

namespace AsvsIntegratedDemo.Controllers {
    public class BusinessLogicController : Controller {
        public IActionResult Index() => View();

        // بند OWASP ASVS: V11.1.1 - Rate Limiting
        public IActionResult RateLimitDemo() {
            ViewBag.Message = "این صفحه توسط RateLimitingMiddleware محافظت می‌شود. بیش از ۵ بار رفرش کنید تا محدودیت اعمال شود (V11.1.1).";
            ViewBag.IsSuccess = true;
            return View("Result");
        }
    }
}
