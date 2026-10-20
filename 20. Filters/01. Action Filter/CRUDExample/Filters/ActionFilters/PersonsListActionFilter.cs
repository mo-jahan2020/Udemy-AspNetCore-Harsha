using Microsoft.AspNetCore.Mvc.Filters;

namespace CRUDExample.Filters.ActionFilters
{
 public class PersonsListActionFilter : IActionFilter
 {
  private readonly ILogger<PersonsListActionFilter> _logger;

  public PersonsListActionFilter(ILogger<PersonsListActionFilter> logger)
  {
   _logger = logger;
  }
        // IActionFilter interface has two methods: OnActionExecuting and OnActionExecuted
        // OnActionExecuting method is called before the action method is called
        // این کارها را میتوان در این متد انجام داد: لاگ کردن، اعتبارسنجی، تغییر پارامترهای ورودی و غیره
        public void OnActionExecuted(ActionExecutedContext context)
  {
   //To do: add after logic here
   _logger.LogInformation("PersonsListActionFilter.OnActionExecuted method");
  }
        // OnActionExecuted method is called after the action method is called
        // از جمله کارهایی که در این متد میتوان انجام داد : لاگ کردن، تغییر نتیجه خروجی، تغییر وضعیت پاسخ و غیره
        public void OnActionExecuting(ActionExecutingContext context)
  {
   //To do: add before logic here
   _logger.LogInformation("PersonsListActionFilter.OnActionExecuting method");
  }
 }
}
