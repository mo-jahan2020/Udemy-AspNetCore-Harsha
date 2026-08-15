using System.Text.RegularExpressions; // فضای نام لازم برای استفاده از عبارت‌های منظم (Regular Expressions)

namespace RoutingExample.CustomConstraints;

// مثال کاربرد: آدرسی مانند sales-report/2040/apr 
// این کلاس یک "محدودیت مسیر سفارشی" (Custom Route Constraint) را پیاده‌سازی می‌کند.
// هدف: اطمینان از اینکه پارامتر مسیر (مثلاً نام ماه) فقط می‌تواند مقادیر مشخصی را داشته باشد.
public class MonthsCustomConstraint : IRouteConstraint
{
    // متد Match قلب تپندهٔ اینترفیس IRouteConstraint است.
    // این متد برای هر درخواست ورودی اجرا می‌شود و اگر true برگرداند، یعنی مسیر معتبر است و به کنترلر هدایت می‌شود.
    public bool Match(HttpContext? httpContext, IRouter? route, string routeKey, RouteValueDictionary values,
        RouteDirection routeDirection)
    {
        // ۱. بررسی وجود کلید: ابتدا چک می‌کنیم که آیا پارامتر مورد نظر (مثلاً "month") در مقادیر مسیر وجود دارد یا خیر.
        // routeKey نام پارامتری است که این محدودیت روی آن اعمال شده (مثلاً در الگوی {month:monthsCustom})
        if (!values.ContainsKey(routeKey))
        {
            return false; // اگر کلید وجود نداشت، قطعاً تطابقی صورت نگرفته است.
        }

        // ۲. تعریف الگوی عبارت منظم (Regex):
        // ^ : نشان‌دهندهٔ شروع رشته است.
        // $ : نشان‌دهندهٔ پایان رشته است.
        // | : عملگر منطقی "یا" (OR) است.
        // نتیجه: این الگو فقط و فقط رشته‌هایی را می‌پذیرد که دقیقاً و منحصراً "apr"، "jul"، "oct" یا "jan" باشند.
        Regex regex = new Regex($"^(apr|jul|oct|jan)$");

        // ۳. استخراج مقدار: مقدار پارامتر را از دیکشنری خوانده و به رشته (string) تبدیل می‌کنیم.
        string? monthValue = Convert.ToString(values[routeKey]);

        // ۴. بررسی تطابق: متد IsMatch بررسی می‌کند که آیا مقدار استخراج شده با الگوی منظم ما همخوانی دارد یا خیر.
        if (regex.IsMatch(monthValue))
        {
            return true; // در صورت تطابق، مسیر معتبر تشخیص داده می‌شود.
        }

        // در صورت عدم تطابق (مثلاً کاربر وارد کند "may" یا "february")، مقدار false بازگردانده می‌شود 
        // و ASP.NET Core مسیر (Route) دیگری را جستجو می‌کند یا خطای 404 برمی‌گرداند.
        return false;
    }
}