using Microsoft.AspNetCore.Mvc;
using ModelValidationsExample.CustomModelBinders;
using ModelValidationsExample.Models;

namespace ModelValidationsExample.Controllers
{
    public class HomeController : Controller
    {
        [Route("register")]
        // اگر از این بصورت تنها استفاده میکردیم: Person person
        // برای تبدیل (bind) داده‌های ورودی به آبجکت Person از کلاس پیش فرض آن استفاده کرده بودیم

        // ولی این [ModelBinder(...)] : از binder سفارشی استفاده کن
        //typeof(PersonModelBinder):  این کلاس را به‌عنوان binder استفاده کن  
        //Person person : نتیجه را در این پارامتر بریز
        // for test in postman http://localhost:5166/register
        // body->form-data->
        /*
        FirstName = John
    	LastName = Doe
        Email = john.doe@example.com
    	Phone = 1234567890
    	Password = Secret123
    	ConfirmPassword = Secret123
    	Price = 12.34
    	DateOfBirth = 1990-01-01
        */
        public IActionResult Index([ModelBinder(typeof(PersonModelBinder))] Person person)
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
