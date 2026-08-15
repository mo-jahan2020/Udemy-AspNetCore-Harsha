using Microsoft.AspNetCore.Mvc;
using ControllersExample.Models;

namespace ControllersExample.Controllers
{
    public class HomeController : Controller
    {
        [Route("home")]
        [Route("/")]
        public ContentResult Index()
        {
            //return new ContentResult() { Content = "Hello from Index", ContentType = "text/plain" };

            //return Content("Hello from Index", "text/plain");

            return Content("<h1>Welcome</h1> <h2>Hello from Index</h2>", "text/html");
        }

        [HttpGet]
        [Route("person")]
        public JsonResult Person()
        {
            Person person = new Person() { Id = Guid.NewGuid(), FirstName = "James", LastName = "Smith", Age = 25 };
            //return new JsonResult(person);
            return Json(person);
            //return "{ \"key\": \"value\" }";
        }

        [Route("file-download")]
        public VirtualFileResult FileDownload()
        {
            //default path for /sample.pdf is wwwroot folder
            return new VirtualFileResult("/sample.pdf", "application/pdf");
            //OR
            //return File("/sample.pdf", "application/pdf");
        }

    }
}
