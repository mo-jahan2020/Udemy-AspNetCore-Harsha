using Microsoft.AspNetCore.Mvc;

namespace AsvsIntegratedDemo.Controllers {
    public class RequestProtectionController : Controller {
        public IActionResult Index() => View();

        // بند OWASP ASVS: V8.1.1 - محافظت CSRF
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CsrfAction() {
            ViewBag.Message = "درخواست با اعتبارسنجی توکن Anti-CSRF پردازش شد (V8.1.1).";
            ViewBag.IsSuccess = true;
            return View("Result");
        }
    }
}
