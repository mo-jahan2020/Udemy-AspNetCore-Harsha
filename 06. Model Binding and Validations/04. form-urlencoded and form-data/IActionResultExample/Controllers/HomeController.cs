using Microsoft.AspNetCore.Mvc;
using IActionResultExample.Models;

namespace IActionResultExample.Controllers
{
  public class HomeController : Controller
  { 
    [Route("bookstore/{bookid?}/{isloggedin?}")]
        //قوانین
        //bookid Model Binding پیش‌فرض اولویت: Route → Query
        //isloggedin فقط از Route
        //Book : Form data[body]->Route data->Query String->Files

        //چندمثال
        ///bookstore/1/false?bookid=20&isloggedin=true&author=harsha
        //bookid = 1 (از Route)
        //isloggedin = false (از Route)
        //bookid : 1 (از Route)
        //Book.BookId = 1 Route (قسمت اول)
        //Book.Author = harsha چون روت موجود نیست از کوری میگیرد

        ///bookstore/500/true?author=harsha
        //bookid = 500 (از Route)
        //isloggedin = true (از Route)
        //book.BookId = 500 (از Route)
        //book.Author = harsha(از Query، چون در Route نبود)

        ///bookstore?bookid=50&isloggedin=true&author=Ali
        //bookid → چون در Route نبود، فرآیند fallback میره سراغ Query → 50
        //isloggedin → چون[FromRoute] است و در Route چیزی نیست، null میشه(حتی با اینکه در Query مقدار true هست، دیده نمیشه!)
        //book.BookId → 50 (از Query، چون Route نداشت)
        //book.Author → Ali
        
        public IActionResult Index(int? bookid, [FromRoute]bool? isloggedin, Book book)
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

      return Content($"Book id: {bookid},isloggedin: {isloggedin}, Book: {book}", "text/plain");
    }
  }
}
