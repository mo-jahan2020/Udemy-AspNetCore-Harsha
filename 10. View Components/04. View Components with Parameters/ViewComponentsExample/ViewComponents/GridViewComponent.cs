using Microsoft.AspNetCore.Mvc;
using ViewComponentsExample.Models;

namespace ViewComponentsExample.ViewComponents
{
  public class GridViewComponent : ViewComponent
  {
        // کلاس اختصاصی پرسون دریافت شده
    public async Task<IViewComponentResult> InvokeAsync(PersonGridModel grid)
    {
            //و برای نمایش به ویو سمپل پاس داده میشود
      return View("Sample", grid); //invokes a partial view Views/Shared/Components/Grid/Sample.cshtml
    }
  }
}
