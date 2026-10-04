using Microsoft.AspNetCore.Mvc;

namespace ConfigurationExample.Controllers
{
    public class HomeController : Controller
    {
        //private field
        private readonly IConfiguration _configuration;

        //constructor
        // The IConfiguration interface is injected into the constructor of the HomeController class.
        // This allows the controller to access configuration settings defined in appsettings.json or other configuration sources.
        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Route("/")]
        public IActionResult Index()
        {
            //ViewBag.ClientID = _configuration["weatherapi:ClientID"];
            //ViewBag.ClientSecret = _configuration.GetValue("weatherapi:ClientSecret", "the default client secret");


            // Get the "weatherapi" section from the configuration
            IConfigurationSection wetherapiSection = _configuration.GetSection("weatherapi");

            // Retrieve the "ClientID" and "ClientSecret" values from the "weatherapi" section
            ViewBag.ClientID = wetherapiSection["ClientID"];
            ViewBag.ClientSecret = wetherapiSection["ClientSecret"];

            return View();
        }
    }
}
