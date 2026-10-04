using Microsoft.AspNetCore.Mvc;

namespace EnvironmentsExample.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        //constructor
        public HomeController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        
        [Route("/")]

        public IActionResult Index()
        {
            // مقدار اینوایرومنت را به ویو پاس میدهیم
            
            ViewBag.CurrentEnviornment = _webHostEnvironment.EnvironmentName;
            return View();
        }
        // این یک خطا است که در داخل پروگرام آن را هندل کرده ایم
        //[Route("/")] // اگر این کامنت را بردارید خطا رخ خواهد داد
        public IActionResult other()
        {
            ViewBag.CurrentEnviornment = _webHostEnvironment.EnvironmentName;
            return View();
        }

    }
}
