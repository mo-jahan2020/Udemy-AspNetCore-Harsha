using Microsoft.AspNetCore.Mvc;

namespace AsvsIntegratedDemo.Controllers {
    public class SessionController : Controller {
        public IActionResult Index() => View();

        // بند OWASP ASVS: V3.2.1 - تنظیم کوکی‌های نشست با فلگ‌های امنیتی
        [HttpPost]
        public IActionResult CreateSecureSession() {
            var cookieOptions = new CookieOptions {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddMinutes(15)
            };
            Response.Cookies.Append("ASVS_SessionId", Guid.NewGuid().ToString(), cookieOptions);
            ViewBag.Message = "کوکی امن با فلگ‌های HttpOnly, Secure, SameSite تنظیم شد (V3.2.1).";
            ViewBag.IsSuccess = true;
            return View("Result");
        }
    }
}
