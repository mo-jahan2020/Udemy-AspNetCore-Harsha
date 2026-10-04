using Microsoft.AspNetCore.Mvc;
using AsvsIntegratedDemo.Services;

namespace AsvsIntegratedDemo.Controllers {
    public class AuthorizationController : Controller {
        private readonly IUserService _userService;
        public AuthorizationController(IUserService userService) { _userService = userService; }

        public IActionResult Index() => View();

        // بند OWASP ASVS: V4.1.1 - کنترل دسترسی
        [HttpPost]
        public async Task<IActionResult> CheckAccess(string username) {
            bool isAdmin = await _userService.IsAdminAsync(username);
            ViewBag.Message = isAdmin ? $"کاربر {username} دسترسی ادمین دارد (V4.1.1)." : $"کاربر {username} دسترسی ادمین ندارد.";
            ViewBag.IsSuccess = isAdmin;
            return View("Result");
        }
    }
}
