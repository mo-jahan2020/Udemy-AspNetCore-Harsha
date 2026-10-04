using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ConfigurationExample.Controllers
{
    public class HomeController : Controller
    {
        //private field
        private readonly WeatherApiOptions _options;

        //constructor
        // The constructor takes an IOptions<WeatherApiOptions> parameter, which is used to retrieve the configuration values for the WeatherApiOptions class.
        // The values are then stored in a private field for later use in the controller's actions.
        // The IOptions<T> interface is part of the Microsoft.Extensions.Options namespace and is used to retrieve configuration values_ 
        // from the appsettings.json file or other configuration sources.
        public HomeController(IOptions<WeatherApiOptions> weatherApiOptions)
        {
            _options = weatherApiOptions.Value;
        }

        [Route("/")]
        public IActionResult Index()
        {
            ViewBag.ClientID = _options.ClientID;
            ViewBag.ClientSecret = _options.ClientSecret;

            return View();
        }
    }
}
