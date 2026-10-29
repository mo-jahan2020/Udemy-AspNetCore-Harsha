using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CRUDExample.Controllers
{
 public class HomeController : Controller
 {
        // کاربر لاگ این نکرده به این متد هم دسترسی دارد
        //مهم این است این را نباید روی خود کنترلر بگذاریم
  [AllowAnonymous]
  [Route("Error")]
  public IActionResult Error()
  {
   IExceptionHandlerPathFeature ? exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
   if (exceptionHandlerPathFeature != null && exceptionHandlerPathFeature.Error != null)
   {
    ViewBag.ErrorMessage = exceptionHandlerPathFeature.Error.Message;
   }
   return View(); //Views/Shared/Error
  }
 }
}
