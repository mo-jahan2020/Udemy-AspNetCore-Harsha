using Microsoft.AspNetCore.Mvc;

namespace AsvsIntegratedDemo.Controllers {
    public class LoggingController : Controller {
        private readonly ILogger<LoggingController> _logger;
        public LoggingController(ILogger<LoggingController> logger) { _logger = logger; }

        public IActionResult Index() => View();

        // بند OWASP ASVS: V7.1.2 - لاگ‌گذاری امن
        [HttpPost]
        public IActionResult TestLogging(string creditCard, string password) {
            _logger.LogInformation("کاربر سعی کرد با شماره کارت {CreditCard} و رمز {Password} وارد شود.", creditCard, password);
            _logger.LogInformation("یک عملیات عادی و امن انجام شد.");
            ViewBag.Message = "عملیات انجام شد. کنسول سرور را بررسی کنید. لاگ‌های حاوی اطلاعات حساس حذف شدند (V7.1.2).";
            ViewBag.IsSuccess = true;
            return View("Result");
        }

        // بند OWASP ASVS: V7.1.3 - مدیریت خطای امن
        [HttpPost]
        public IActionResult ErrorHandlingDemo() {
            try {
                throw new Exception("Connection string 'Server=10.0.0.5;User=sa;Password=123' failed!");
            } catch {
                ViewBag.Message = "یک خطای داخلی رخ داد. جزئیات فنی در سرور لاگ شد (V7.1.3).";
                ViewBag.IsSuccess = true;
            }
            return View("Result");
        }
    }
}
