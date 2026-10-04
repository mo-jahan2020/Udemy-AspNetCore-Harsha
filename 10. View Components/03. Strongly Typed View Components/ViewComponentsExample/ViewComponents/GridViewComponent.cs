using Microsoft.AspNetCore.Mvc;
using ViewComponentsExample.Models;

namespace ViewComponentsExample.ViewComponents
{
  public class GridViewComponent : ViewComponent
  {
    public async Task<IViewComponentResult> InvokeAsync()
    {
      PersonGridModel personGridModel = new PersonGridModel()
      {
        GridTitle = "Persons List",
        Persons = new List<Person>() {
          new Person() { PersonName = "John", JobTitle = "Manager" },
          new Person() { PersonName = "Jones", JobTitle = "Asst. Manager" },
          new Person() { PersonName = "William", JobTitle = "Clerk" },
        }
      };
            // در برنامه قبلی به این شکل مدل پاس داده شده است
            // ViewData["Grid"] = model;
            //return View("Sample");
            // ولی حالا مستقیم با ویو مدل آن هم به ویو ارسال میشود
            return View("Sample", personGridModel); //invoked a partial view Views/Shared/Components/Grid/Default.cshtml
    }
  }
}
