using AsvsLab.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AsvsLab.Controllers;

// ============================================================================
// 🏠 صفحه اصلی: داشبورد دکمه‌های دمو
// ============================================================================
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    // داشبورد اصلی با همه دکمه‌های دمو
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    // ------------------------------------------------------------------------
    // ⚙️ بند V7.4.1 استاندارد OWASP ASVS 4.0.3:
    // «هنگام خطای غیرمنتظره فقط پیام عمومی نمایش داده شود»
    // این صفحه به‌عنوان «آخرین خط دفاعی» (بند V7.4.3) در Program.cs ثبت شده است
    // ------------------------------------------------------------------------
    [HttpGet]
    public IActionResult Error()
    {
        return View("../Shared/Error", new DemoResult
        {
            Title = "خطای غیرمنتظره",
            Explanation = "یک خطای غیرمنتظره رخ داد. جزئیات کامل در لاگ سرور ثبت شد و برای تیم پشتیبانی قابل پیگیری است (بند V7.4.1). لطفاً از دکمه بازگشت استفاده کنید.",
            BackController = "Home"
        });
    }
}
