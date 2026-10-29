using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net.Mime;

namespace CRUDExample.Filters.AuthorizationFilter
{
 public class TokenAuthorizationFilter : IAuthorizationFilter
 {
        // این فیلتر اولین فیلتر است که اجرا میشود و فقط در اول اجرا میشود و یک طرفه است و آخر ندارد
  public void OnAuthorization(AuthorizationFilterContext context)
  {
            // اگر کلید را پیدا نکرد خطا میدهد
   if (context.HttpContext.Request.Cookies.ContainsKey("Auth-Key") == false)
   {
    context.Result = new StatusCodeResult(StatusCodes.Status401Unauthorized);
    return;
   }
   // کلید را پیدا کرد ولی مقدار آن برابر با کلید ما نبود با زهم خطا میدهد
   if (context.HttpContext.Request.Cookies["Auth-Key"] != "A100")
   {
    context.Result = new StatusCodeResult(StatusCodes.Status401Unauthorized);
   }
            // context.Result ها باعث میشوند مقادیر View ها با این مقادیر جایگزین شده و آنها دیگر اصولاً اجرا نمیشوند
        }
    }
}
