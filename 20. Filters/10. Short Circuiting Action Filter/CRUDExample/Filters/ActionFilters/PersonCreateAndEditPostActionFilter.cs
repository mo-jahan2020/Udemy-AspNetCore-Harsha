using CRUDExample.Controllers;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using ServiceContracts;
using ServiceContracts.DTO;

namespace CRUDExample.Filters.ActionFilters
{
    public class PersonCreateAndEditPostActionFilter : IAsyncActionFilter
    {
        private readonly ICountriesService _countriesService;

        public PersonCreateAndEditPostActionFilter(ICountriesService countriesService)
        {
            _countriesService = countriesService;
        }

        //context اینجا از نوع ActionExecutingContext است، یعنی لحظه‌ی قبل از اجرای اکشن را برمیگرداند
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            //TO DO: before logic mohem

            if (context.Controller is PersonsController personsController)
            {
                if (!personsController.ModelState.IsValid)
                {
                    // اگر شرط برقرار نباشد اصولاً وارد اکشن Create or Delete Post نمیشود و همین جا تکلیف را مشخص میکند
                    List<CountryResponse> countries = await _countriesService.GetAllCountries();
                    personsController.ViewBag.Countries = countries.Select(temp =>
                    new SelectListItem() { Text = temp.CountryName, Value = temp.CountryID.ToString() });

                    personsController.ViewBag.Errors = personsController.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    //ActionArguments یک دیکشنری است که تمام پارامترهایی که قرار است به اکشن متد پاس داده شوند را نگه می‌دارد.
                    //کلید این دیکشنری، نام پارامتر اکشن است یعنی اگر دقت کنید هر دو اکشن افزودن و ویرایش دارای این پارامتر ورودی هستند
                    //پس context.ActionArguments["personRequest"] یعنی: همان داده‌ای که کاربر در فرم وارد کرده و توسط
                    //Model Binding به شیء PersonAddRequest تبدیل شده را به من بده.
                    var personRequest = context.ActionArguments["personRequest"];
                    //این خط دو کار همزمان انجام می‌دهد:
                    //۱. Short-Circuit (قطع زنجیره‌ی اجرا)
                    //وقتی در فیلترِ قبل از اکشن، مقدار context.Result را ست کنید:
                    //اکشن متد (CreatePost یا EditPost) اصلاً اجرا نمی‌شود
                    //فیلترهای بعدی هم رد می‌شوند
                    //مستقیماً ViewResult تولید شده به پاسخ(Response) می‌رود
                    //برای همین در کامنت نوشته شده short-circuits.اگر به جای ست کردن context.Result،
                    //فقط await next() را صدا بزنیم، جریان عادی ادامه پیدا می‌کند(دقیقاً همان کاری که در بخش else انجام شده).
                    //چرا personRequest را به View() پاس دادیم؟
                    //چون ModelState.IsValid مقدار false بوده، یعنی داده‌های فرم خطا دارند.
                    //اگر مدل را به View ندهیم، فرم خالی به کاربر نشان داده می‌شود و همه‌ی اطلاعاتی که وارد کرده بود مثل نام، ایمیل، تاریخ تولد پاک می‌شود.
                    context.Result = personsController.View(personRequest); //short-circuits or skips the subsequent action filters & action method
                }
                else
                {
                    // اگر دیتای وارد شده درست باشد به مرحله بعد میرود
                    // اگر دیتا وارد شده درست بود به فیلتر بعدی رفته یا اکشن مورد نظر اجرا میشود

                    await next(); //invokes the subsequent filter or action method
                }
            }
            else
            {
                //این بلاک زمانی اجرا می‌شود که کنترلرِ در حال اجرا اصلاً PersonsController نباشد.
                //یعنی فیلترِ شما روی اکشنی از یک کنترلر دیگر مثلاً HomeController یا AccountController در حال اجراست.
                await next(); //calls the subsequent filter or action method
            }

            //TO DO: before logic
        }
    }
}
