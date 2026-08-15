using Microsoft.AspNetCore.Mvc;

namespace ControllersExample.Controllers
{
    [Controller]
    public class productsController
    {
        // /api/products OR  OR /products/home   OR  /products/
        [HttpGet]// اجباری نیست ولی اگر نوشته نشود به تمام متد ها پاسخ خواهدد داد
        // وجود خط فوق یعنی فقط به گت پاسخ دهد
        [Route("api/[controller]")]
        [Route("[controller]/home")]
        [Route("[controller]/")]
        public string Index()
        {
            return "Hello from products Index";
        }

        //http://localhost:5151/products/about POST by postman of From Post Method
        [HttpPost] // اینجا دیگر فقط به پست جواد خواهد داد نه دیگر متد ها
        [Route("[controller]/about")]
        public string About()
        {
            return "Hello from products About";
        }
        //http://localhost:5151/contact-us/0918215433
        // این به تمام متد ها پاسخ خوهد داد چون نگفته ایم به چه پاسخ دهد
        [Route("[controller]/contact-us/{mobile:regex(^\\d{{10}}$)}")]
        public string Contact(string? mobile)
        {
            return $"Hello from products Contact : {mobile}";
        }
    }
}
