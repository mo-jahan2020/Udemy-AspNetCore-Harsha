using Microsoft.AspNetCore.Mvc;

namespace IActionResultExample.Controllers
{
    public class StoreController : Controller
    {
        //[Route("store/books")]
        //OR
        [Route("[Controller]/books")]
        public IActionResult Books()
        {
            return Content("<h1>Book Store</h1>", "text/html");
        }
    }
}
