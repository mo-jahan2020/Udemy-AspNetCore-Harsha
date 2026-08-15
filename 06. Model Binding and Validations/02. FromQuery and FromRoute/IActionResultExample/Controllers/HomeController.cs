using Microsoft.AspNetCore.Mvc;

namespace IActionResultExample.Controllers
{
    public class HomeController : Controller
    {
        [Route("books/{bookid?}/{isloggedin?}")]
        //Url: /books/isloggedin/true/?bookid=10
        /*
         [FromQuery] int? bookid
        این پارامتر قرار است از Query String بعد از ? در URL دریافت شود.
        مثال: /books?bookid=5

        [FromRoute] bool? isloggedin
        این پارامتر قرار است از Route Data قسمتی از مسیر دریافت شود.
        مثال: /books/true
         */
        public IActionResult Index([FromQuery] int? bookid, [FromRoute] bool? isloggedin)
        {
            //Book id should be applied
            if (bookid.HasValue == false)
            {
                //return new BadRequestResult();
                return BadRequest("Book id is not supplied or empty");
            }

            //Book id can't be less than or equal to 0
            if (bookid <= 0)
            {
                return BadRequest("Book id can't be less than or equal to 0");
            }

            //Book id should be between 1 to 1000
            if (bookid > 1000)
            {
                return NotFound("Book id can't be greater than 1000");
            }

            //isloggedin should be true
            if (isloggedin == false)
            {
                return StatusCode(401);
            }

            return Content($"Book id: {bookid} isloggedin:{isloggedin} " , "text/plain");
        }
    }
}
