using Microsoft.AspNetCore.Mvc;

namespace ControllersExample.Controllers
{
    [Controller]
    public class HomeController
    {
        [Route("home")]
        [Route("/")]
        public string Index()
        {
            return "Hello from Index";
        }

        [Route("about")]
        public string About()
        {
            return "Hello from About";
        }
        //http://localhost:5151/contact-us/0918215433
        [Route("contact-us/{mobile:regex(^\\d{{10}}$)}")]
        public string Contact(string? mobile)
        {
            return $"Hello from Contact : {mobile}";
        }
    }



}
