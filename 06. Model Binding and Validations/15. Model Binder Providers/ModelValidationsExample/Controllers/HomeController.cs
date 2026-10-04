using Microsoft.AspNetCore.Mvc;
using ModelValidationsExample.CustomModelBinders;
using ModelValidationsExample.Models;

namespace ModelValidationsExample.Controllers
{
 public class HomeController : Controller
 {
  [Route("register")]
  // اگر اینکار را نمیکردیم باید  این خظ زیر را با مقدار داخل پرانتز ایندکس در تمام برنامه عوض میکردیم
  //[ModelBinder(BinderType = typeof(PersonModelBinder))]
  public IActionResult Index(Person person)
  {
   if (!ModelState.IsValid)
   {
    //get error messages from model state
    string errors = string.Join("\n", ModelState.Values.SelectMany(value => value.Errors).Select(err => err.ErrorMessage));

    return BadRequest(errors);
   }

   return Content($"{person}");
  }
 }
}
