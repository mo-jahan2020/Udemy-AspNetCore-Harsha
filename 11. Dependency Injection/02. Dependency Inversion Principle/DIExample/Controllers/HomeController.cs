using Microsoft.AspNetCore.Mvc;
using Services;
using ServiceContracts;

namespace DIExample.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICitiesService _citiesService;

        //constructor
        public HomeController()
        {
            //create object of CitiesService class
            //_citiesService = null; //new CitiesService();
        }

        [Route("/")]
        public IActionResult Index()
        {
            // اجرای برنامه در اینجا خطا خواهد داد زیرا تزریق وابستگی در برنامه صورت نگرفته است .
            // در درس بعدی این مشکل حل خواهد شد
            List<string> cities = _citiesService.GetCities();
            return View(cities);
        }
    }
}
