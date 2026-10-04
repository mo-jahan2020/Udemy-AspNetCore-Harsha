using Microsoft.AspNetCore.Mvc;
using ModelValidationsExample.Models;
using ModelValidationsExample.CustomModelBinders;

namespace ModelValidationsExample.Controllers
{
  public class HomeController : Controller
  {
    [Route("register")]
        // user-agent پیش فرض از طرف فرستنده در هدر ست میشود
        //بجای مقدار فوق میتوان KEY گذاشت .
        // در آن صورت باید در هدر پوستمن باید آن را مقدار دهی شود
        public IActionResult Index(Person person, [FromHeader(Name = "User-Agent")] string UserAgent)
    {
      if (!ModelState.IsValid)
      {
        //get error messages from model state
        string errors = string.Join("\n", ModelState.Values.SelectMany(value => value.Errors).Select(err => err.ErrorMessage));
  
        return BadRequest(errors);
      }
      // علاوه بر پرسن آن مقدار هم برگشت داده میشود
      return Content($"{person}, {UserAgent}");
    }
  }
}
