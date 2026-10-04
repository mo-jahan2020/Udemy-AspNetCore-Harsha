using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AsvsAdvancedDemo.Services;
using System.Security.Claims;

namespace AsvsAdvancedDemo.Controllers {
    public class HomeController : Controller {
        private readonly ISecureStorageService _secureStorage;
        private readonly IUserService _userService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ISecureStorageService secureStorage, IUserService userService, ILogger<HomeController> logger) {
            _secureStorage = secureStorage;
            _userService = userService;
            _logger = logger;
        }

        public IActionResult Index() => View();

        // دمو V6.1.1: رمزنگاری داده
        [HttpPost]
        public IActionResult EncryptData(string sensitiveData) {
            var encrypted = _secureStorage.Encrypt(sensitiveData);
            var decrypted = _secureStorage.Decrypt(encrypted);
            ViewBag.Encrypted = encrypted;
            ViewBag.Decrypted = decrypted;
            return View(""Result"");
        }

        // دمو V4.1.1: کنترل دسترسی
        [HttpPost]
        public async Task<IActionResult> CheckAccess(string username) {
            // شبیه‌سازی کاربر لاگین کرده
            bool isAdmin = await _userService.IsAdminAsync(username);
            ViewBag.Message = isAdmin ? $""کاربر {username} دسترسی ادمین دارد (V4.1.1)."" : $""کاربر {username} دسترسی ادمین ندارد. دسترسی رد شد."";
            ViewBag.IsSuccess = isAdmin;
            return View(""Result"");
        }

        // دمو V7.1.2: تست لاگ‌گذاری امن
        [HttpPost]
        public IActionResult TestLogging(string creditCard, string password) {
            // بند V7.1.2: حتی اگر ما عمداً اطلاعات حساس را لاگ کنیم، فیلتر MaskingEnricher جلوی ثبت آن را می‌گیرد
            _logger.LogInformation(""کاربر سعی کرد با شماره کارت {CreditCard} و رمز {Password} وارد شود."", creditCard, password);
            _logger.LogInformation(""یک عملیات عادی و امن انجام شد."");
            
            ViewBag.Message = ""عملیات انجام شد. کنسول سرور را بررسی کنید. لاگ‌های حاوی شماره کارت یا رمز عبور به صورت خودکار حذف/ماسک شدند (V7.1.2)."";
            ViewBag.IsSuccess = true;
            return View(""Result"");
        }
    }
}
