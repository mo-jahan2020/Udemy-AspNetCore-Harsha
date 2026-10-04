using Microsoft.AspNetCore.Mvc;

namespace ViewsExample.Controllers
{
  public class ProductsController : Controller
  {
    [Route("products/all")]
    public IActionResult All()
    {
      return View();
      //Views/Products/All.cshtml
      //نکته : هر ویویی که در شیرد ساخته شود از تمام اکشن های تمام کنترلر ها در دسترس است
      // مسیر پیش فرض بالا است اگر آن را پیدا نکرد در مسیر پایین دنبال آن ویو خواهد گشت 
      //Views/Shared/All.cshtml
    }
  }
}
