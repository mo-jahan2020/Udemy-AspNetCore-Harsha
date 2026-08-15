using Microsoft.AspNetCore.Mvc;

namespace IActionResultExample.Controllers
{
    public class HomeController : Controller
    {

        // IActionResult can return ContentResult , JsonResult , RedirecrResult , StatusResult , ViewResult 
        // زیرا همه موارد ذکر شده بالا نوعی IActionResual میباشند
        [Route("book")]
        public IActionResult Index()
        {
            //book/?isloggedin=true&bookid=1
            //Book id should be applied همه شروط زیر برقرار بود شروع به دانلود میکند
            if (!Request.Query.ContainsKey("bookid"))
            {
                Response.StatusCode = 400;
                //IS ContentResult 
                return Content("Book id is not supplied");
            }

            //Book id can't be empty
            if (string.IsNullOrEmpty(Convert.ToString(Request.Query["bookid"])))
            {
                Response.StatusCode = 400;
                return Content("Book id can't be null or empty");
            }

            //Book id should be between 1 to 1000
            int bookId = Convert.ToInt16(ControllerContext.HttpContext.Request.Query["bookid"]);
            if (bookId <= 0)
            {
                Response.StatusCode = 400;
                return Content("Book id can't be less than or equal to zero");
            }
            if (bookId > 1000)
            {
                Response.StatusCode = 400;
                return Content("Book id can't be greater than 1000");
            }

            //isloggedin should be true
            if (Convert.ToBoolean(Request.Query["isloggedin"]) == false)
            {
                Response.StatusCode = 401;
                return Content("User must be authenticated");
            }

            return File("/sample.pdf", "application/pdf");
        }
    }
}
