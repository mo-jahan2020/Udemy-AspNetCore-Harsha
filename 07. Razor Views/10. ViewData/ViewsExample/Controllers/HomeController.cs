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
            // این خصوصیت بطور خودکار هنگام ساخته شدن کنترلر مقدار دهی میشود
            // هر تعداد اندیس میتوانید به آن نسبت دهید و داخل ویو از آن استفاده کنید
            ViewData["appTitle"] = "Asp.Net Core Demo App";

            List<Person> people = new List<Person>()
      {
          new Person() { Name = "John", DateOfBirth = DateTime.Parse("2000-05-06"), PersonGender = Gender.Male},
          new Person() { Name = "Linda", DateOfBirth = DateTime.Parse("2005-01-09"), PersonGender = Gender.Female},
          new Person() { Name = "Susan", DateOfBirth = DateTime.Parse("2008-07-12"), PersonGender = Gender.Other}
      };
            ViewData["people"] = people;

            return View(); //Views/Home/Index.cshtml
        }
    }
}
