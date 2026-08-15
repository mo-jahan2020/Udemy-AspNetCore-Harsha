using Microsoft.AspNetCore.Mvc;
using ModelValidationsExample.Models;

namespace ModelValidationsExample.Controllers
{
    public class HomeController : Controller
    {
        [Route("register")]
        public IActionResult Index(Person person)
        {
            person.PersonName = "ali";
            /*
             * دیگر اینجا اثر ندارد زیرا ترتیب اولویت کارها به این شکل است
              1. درخواست HTTP میاد
              2. Model Binding اتفاق می‌افته (مقادیر از Form/Query/Route به Person map میشن)
              3. Validation روی مقادیر بایند شده انجام میشه و نتیجه در ModelState ذخیره میشه
              4. بدنه‌ی اکشن (Action Method) اجرا میشه  ← اینجا شما person.PersonName = "ali" رو ست می‌کنید
              5. کد شما ModelState.IsValid رو چک می‌کنه
             */
            if (!ModelState.IsValid)
            {
                List<string> ListError = new List<string>();
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        ListError.Add(error.ErrorMessage);
                    }
                }
                string errors = string.Join('\n', ListError);
                return BadRequest(errors);
                
            }
            return Content($"{person}");
        }
    }
}
