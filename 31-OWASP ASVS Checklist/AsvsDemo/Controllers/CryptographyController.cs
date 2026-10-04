using Microsoft.AspNetCore.Mvc;
using AsvsIntegratedDemo.Services;

namespace AsvsIntegratedDemo.Controllers {
    public class CryptographyController : Controller {
        private readonly ISecureStorageService _secureStorage;
        public CryptographyController(ISecureStorageService secureStorage) { _secureStorage = secureStorage; }

        public IActionResult Index() => View();

        // بند OWASP ASVS: V6.1.1 - رمزنگاری داده‌های حساس
        [HttpPost]
        public IActionResult EncryptData(string sensitiveData) {
            ViewBag.Encrypted = _secureStorage.Encrypt(sensitiveData);
            ViewBag.Decrypted = _secureStorage.Decrypt(ViewBag.Encrypted);
            ViewBag.IsSuccess = true;
            return View("Result");
        }
    }
}
