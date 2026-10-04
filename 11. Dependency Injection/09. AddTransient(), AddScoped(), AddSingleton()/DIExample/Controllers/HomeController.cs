using Microsoft.AspNetCore.Mvc;
using Services;
using ServiceContracts;

namespace DIExample.Controllers
{
  public class HomeController : Controller
  {
    private readonly ICitiesService _citiesService1;
    private readonly ICitiesService _citiesService2;
    private readonly ICitiesService _citiesService3;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    //constructor
    public HomeController(ICitiesService citiesService1, ICitiesService citiesService2, ICitiesService citiesService3, IServiceScopeFactory serviceScopeFactory)
    {
      _citiesService1 = citiesService1;
      _citiesService2 = citiesService2;
      _citiesService3 = citiesService3;
      _serviceScopeFactory = serviceScopeFactory;
    }


    [Route("/")]
    public IActionResult Index()
    {
      List<string> cities = _citiesService1.GetCities();
      ViewBag.InstanceId_CitiesService_1 = _citiesService1.ServiceInstanceId;

      ViewBag.InstanceId_CitiesService_2 = _citiesService2.ServiceInstanceId;

      ViewBag.InstanceId_CitiesService_3 = _citiesService3.ServiceInstanceId;

            /*
             * این کد به‌صورت دستی یک Scope جدید می‌سازد 
             * و از داخل آن یک نمونه از ICitiesService می‌گیرد.
             * معمولاً 
             * این الگو برای آزمایش و نمایش تفاوت رفتار Lifetime‌ها (Transient / Scoped / Singleton) 
             * در یک Controller نوشته می‌شود — یعنی می‌خواهید نشان دهید که یک سرویس Scoped وقتی از یک
             * Scope دستی جدید گرفته شود،
             * نمونه‌ی متفاوتی نسبت به سرویسی دارد که DI به‌صورت خودکار در Constructor کنترلر تزریق کرده است.
             * */
            using (IServiceScope scope = _serviceScopeFactory.CreateScope())
      {
        //Inject CitiesService
        ICitiesService citiesService = scope.ServiceProvider.GetRequiredService<ICitiesService>();
        //DB work

        ViewBag.InstanceId_CitiesService_InScope = citiesService.ServiceInstanceId;
      } //end of scope; it calls CitiesService.Dispose()

      return View(cities);
    }
  }
}
