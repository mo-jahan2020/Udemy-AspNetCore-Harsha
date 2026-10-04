using Microsoft.AspNetCore.Mvc;
using PartialViewsExample.Models;

namespace PartialViewsExample.Controllers
{
    public class HomeController : Controller
    {
        [Route("/")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("about")]
        public IActionResult About()
        {
            return View();
        }

        [Route("programming-languages")]
        public IActionResult ProgrammingLanguages()
        {
            // اگر مستقیم / programming-languages را فراخوانی کنیم برنامه با _ListPartialView مانند یک ویو معمولی برخورد خواهد کرد
            // یعنی این Model ره آن فرستاده و آنجا آن را چاپ خواهد کرد
            ListModel listModel = new ListModel()
            {
                ListTitle = "Programming Languages List1",
                ListItems = new List<string>() {
          "Python",
          "C#",
          "Go"
        }
            };

            return PartialView("_ListPartialView", listModel);
        }
    }
}
