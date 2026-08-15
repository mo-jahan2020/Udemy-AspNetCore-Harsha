using Microsoft.AspNetCore.Mvc;
using ModelValidationsExample.Models;

namespace ModelValidationsExample.Controllers
{
  public class HomeController : Controller
  {
    [Route("register")]
        // (Person person) : تمام آیتم های یک کلاس را لود میکند
        // ولی این دوستور فقط فیلد های اختیاری شما را لود میکند زیرا شاید شما چند فیلد را نمیخواهید لود کنید
        // nameof : می توانستیم بجای این از رشته استفاده کنیم و خوبی این متد از معرفی فیلد این است در صورتی که بعدا در کلاس
        // بخواهید نام یک فیلد را عوض کنید می توانید روی آن راست کلیک کرده و گزینه تعویض نام را بزنید 
        // با این کار بصورت اتوماتیک تمام برنامه که به این شکل تعریف کرده اید آن فیلد تغییر نام خواهد دشات
        // ولی اگر رشته معرفی میکردید باید تمام رشته ها را با دست تغییر نام میدادید
        //Index(Person person)
        //Index([Bind(nameof(Person.PersonName), nameof(Person.Email), nameof(Person.Password), nameof(Person.ConfirmPassword))] Person person)
        public IActionResult Index([Bind(nameof(Person.PersonName), nameof(Person.Email), nameof(Person.Password), nameof(Person.ConfirmPassword),nameof(Person.DateOfBirth), nameof(Person.Age)  )] Person person)
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
