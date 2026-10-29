using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CitiesManager.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController] // این دیگر یک کنترلر وب نیست یک Web API است
    public class TestController : ControllerBase
    {
        [HttpGet]
        // همان index web است
        //http://localhost:5063/api/test?_message=ali1
        // or postman  http://localhost:5063/api/ parametter: _message:ali1
        public string Method(string? _message)
        {
            return "Hello World " + _message;
        }
    }
}
