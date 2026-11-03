using Microsoft.AspNetCore.Mvc;

namespace JwtDemoMVC.Controllers;

// این کنترلر View برمی‌گرداند؛ در Swagger نمایش داده نشود
[ApiExplorerSettings(IgnoreApi = true)]
public class HomeController : Controller
{
    // صفحه‌ی خانه فقط به صفحه‌ی اشخاص هدایت می‌شود (تنها صفحه‌ی اصلی برنامه)
    public IActionResult Index() => RedirectToAction("Index", "Person");
}
