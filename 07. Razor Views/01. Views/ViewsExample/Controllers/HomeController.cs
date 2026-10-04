using Microsoft.AspNetCore.Mvc;


namespace ViewsExample.Controllers
{

    public class HomeController : Controller
    {
        [Route("/")]
        public IActionResult Index()
        {
            return View();// منظور Views/Home/index.cshtml
            //return View("abc"); // منظور Views/Home/abc.cshtml
            //return new ViewResult() { ViewName = "abc" };
        }
    }
}
