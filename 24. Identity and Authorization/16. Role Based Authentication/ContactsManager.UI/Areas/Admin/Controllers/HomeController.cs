using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContactsManager.UI.Areas.Admin.Controllers
{
    // user-base این دسترسی ها فقط روی صفحات اعمال میشود که این یوزر به این صفحه یا تگ خاص دسترسی دارد یا خیر 
 [Area("Admin")]
 // ولی دسترسی role-base روی کنترلر ها اعمال میشود 
 // این خط یعنی نقش ادمین به تمام اکشن های این کنترلر دسترسی داشته باشد
 // برای تست با یوزر عادی وارد شوید و این آدرس را به آخر آدرس موجود اضافه کنید خطا خواهد داد: /admin/home/index
 [Authorize(Roles = "Admin")] // OR "Admin,Manager"
 public class HomeController : Controller
 {
        // اگراینجا میگذاشتیم فقط به این اکشن دسترسی پیدار میکرد
  public IActionResult Index()
  {
   return View();
  }
 }
}
