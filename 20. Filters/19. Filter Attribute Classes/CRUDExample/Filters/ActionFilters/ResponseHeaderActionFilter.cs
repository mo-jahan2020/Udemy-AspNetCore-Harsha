using Microsoft.AspNetCore.Mvc.Filters;

namespace CRUDExample.Filters.ActionFilters
{
    public class ResponseHeaderActionFilter : ActionFilterAttribute // ← کلاس پایه Attribute
    {
        private readonly string _key;
        private readonly string _value;

        public ResponseHeaderActionFilter(string key, string value, int order)
        {
            _key = key;
            _value = value;
            Order = order;// پراپرتی IOrderedFilter از ActionFilterAttribute ارث رسیده
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            await next(); //calls the subsequent filter or action method // فیلتر بعدی یا Action Method

            context.HttpContext.Response.Headers[_key] = _value; // بعد از Action
        }
    }
}
