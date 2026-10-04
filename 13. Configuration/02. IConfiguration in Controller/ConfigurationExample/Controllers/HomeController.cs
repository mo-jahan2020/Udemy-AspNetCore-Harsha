using Microsoft.AspNetCore.Mvc;

namespace ConfigurationExample.Controllers
{
    public class HomeController : Controller
    {
        //private field
        private readonly IConfiguration _configuration;

        //constructor
        //inject IConfiguration into the constructor
        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Route("/")]
        public IActionResult Index()
        {
            //Get the value of MyKey from the configuration
            ViewBag.MyKey = _configuration["MyKey"];
            //Get the value of MyAPIKey from the configuration, with a default value if not found
            ViewBag.MyAPIKey = _configuration.GetValue<string>("MyAPIKey", "the default key");

            return View();
        }
    }
}
