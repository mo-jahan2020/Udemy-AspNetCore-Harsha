using Microsoft.AspNetCore.Mvc;
using IActionResultExample.Models;

namespace IActionResultExample.Controllers
{
  public class HomeController : Controller
  { 
    [Route("bookstore/{bookid?}/{isloggedin?}")]
        //http://localhost:5202/bookstore/?bookid=20&isloggedin=true
        //bookid=20 from Query:?bookid=20 ([FromQuery]int? bookid)
        //isloggedin=null zira isloggedin=true is Query is not Rout ([FromRoute]bool? isloggedin)
        //Books.BookId =20  har chand BookId!=bookid vali books.BookId is [FromQuery] then match
        //در ASP.NET Core، Model Binding به طور پیش‌فرض Case-Insensitive است، یعنی:  bookid = BookId = BOOKID
        //Books.author=null beacuse author is not in 1- Route Data AND Not IN 2- Query String

        //http://localhost:5202/bookstore/?bookid=20&isloggedin=true&author=harsha
        //bookid=20 from Query:?bookid=20 ([FromQuery]int? bookid)
        //isloggedin=null beacuse isloggedin=true is Query is not Rout ([FromRoute]bool? isloggedin)
        //Books.BookId =20  har chand BookId!=bookid vali books.BookId is [FromQuery] then match()
        //در ASP.NET Core، Model Binding به طور پیش‌فرض Case-Insensitive است، یعنی:  bookid = BookId = BOOKID
        //Books.author=harsha beacuse author is not in 1- Route Data AND is IN 2- Query String(Matched)


        //bookid=20 beacuse bookid FromQuery  -- isloggedin=null beacuse isloggedin FromRoute 

        //http://localhost:5202/bookstore/isloggedin/true?bookid=20&author=harsha
        //bookid=20 from Query:?bookid=20 ([FromQuery]int? bookid)
        //isloggedin=true beacuse isloggedin=true is in Route ([FromRoute]bool? isloggedin)
        //Books.BookId =20  har chand BookId!=bookid vali books.BookId is [FromQuery] then match()
        //در ASP.NET Core، Model Binding به طور پیش‌فرض Case-Insensitive است، یعنی:  bookid = BookId = BOOKID
        //Books.author=harsha beacuse author is not in 1- Route Data but is IN 2- Query String(Matched)
        // author=Author Case-Insensitive

        public IActionResult Index([FromQuery]int? bookid, [FromRoute]bool? isloggedin, Book book)
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

      return Content($"Book id: {bookid},isloggedin:{isloggedin}, Book: {book}", "text/plain");
    }
  }
}
