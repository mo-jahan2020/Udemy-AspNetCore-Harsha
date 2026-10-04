using Microsoft.AspNetCore.Mvc;

namespace AsvsIntegratedDemo.Controllers {
    public class HomeController : Controller {
        public IActionResult Index() => View();
    }
}
