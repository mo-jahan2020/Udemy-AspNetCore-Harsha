using Microsoft.AspNetCore.Mvc;
using ViewsExample.Models;

namespace ViewsExample.Controllers
{
  public class HomeController : Controller
  {
    [Route("home")]
    [Route("/")]
    public IActionResult Index()
    {
      ViewData["appTitle"] = "Asp.Net Core Demo App";

      List<Person> people = new List<Person>()
      {
          new Person() { Name = "John", DateOfBirth = DateTime.Parse("2000-05-06"), PersonGender = Gender.Male},
          new Person() { Name = "Linda", DateOfBirth = DateTime.Parse("2005-01-09"), PersonGender = Gender.Female},
          new Person() { Name = "Susan", DateOfBirth = null, PersonGender = Gender.Other}
      };
            //هر دوی این‌ها ابزارهایی هستند برای انتقال داده از Controller به View، اما تفاوت‌های مهمی در نحوه‌ی کار و محدودیت‌هایشان دارند.
            //چون ViewBag فقط یک Wrapper پویا روی ViewData است، این دو یک منبع داده مشترک دارند. یعنی اگر با ViewData مقداری تنظیم کنی، می‌توانی همان را با ViewBag بخوانی و برعکس:
            //ViewData["Message"] = "سلام";
            // معادل است با نوشتن:
            // ViewBag.Message = "سلام";
            //Console.WriteLine(ViewBag.Message); // "سلام" - چون هردو به یک دیکشنری اشاره می‌کنند
            // نوع ViewBag Dynamic است 
            // نوع ViewData از نوع Dictionary<string, object> است


            //ViewData["people"] = people;
            // مقدار دهی People
            ViewBag.people = people;

      return View(); //Views/Home/Index.cshtml
    }
  }
}
