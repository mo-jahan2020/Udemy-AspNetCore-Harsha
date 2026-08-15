using Microsoft.AspNetCore.Mvc;
using ModelValidationsExample.Models;

namespace ModelValidationsExample.Controllers
{
  public class HomeController : Controller
  {
    [Route("register")]
        //enable the input formatter to read data from request body (as JSON OR XML Or Custome) only 
        //Example raw-> JSON: { "PersonName": "William", "Email": "william@example.com", "Phone": "123456", "Password": "william123", "ConfirmPassword": "william123" }
        //Example raw-> XML: Befor ->program.cs ->builder.Services.AddControllers().AddXmlSerializerFormatters();
        /*
         <Person>
            <PersonName>Sarah Connor</PersonName>
            <Email>sarah.connor@example.com</Email>
            <Phone>9123456789</Phone>
            <Password>SecurePass1</Password>
            <ConfirmPassword>SecurePass1</ConfirmPassword>
            <Price>249.99</Price>
            <DateOfBirth>2010-06-15T00:00:00</DateOfBirth>
        </Person>*/
        // postman -> Form-Data -> error code 415  unsupported Media  [FromBody] => [FromForm]
        public IActionResult Index([FromForm] Person person)
    {
      if (!ModelState.IsValid)
      {
        //get error messages from model state
        string errors = string.Join("\n", ModelState.Values.SelectMany(value => value.Errors).Select(err => err.ErrorMessage));
  
        return BadRequest(errors);
      }

      return Content($"{person}");
    }
  }
}
