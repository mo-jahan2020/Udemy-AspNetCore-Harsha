using Microsoft.AspNetCore.Mvc;

namespace CRUDExample.Controllers
{
    public class HomeController : Controller
    {
        [Route("/home1/index")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
