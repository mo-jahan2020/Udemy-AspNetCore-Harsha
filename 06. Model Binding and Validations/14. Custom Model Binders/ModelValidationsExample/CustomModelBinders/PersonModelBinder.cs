using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModelValidationsExample.Models;

namespace ModelValidationsExample.CustomModelBinders
{
    /// <summary>
    /// این کلاس یک Custom Model Binder سفارشی است.
    /// هدف: کنترل دستی نحوهٔ تبدیل داده‌های ورودی (از فرم، کوئری استرینگ و...) به آبجکت Person.
    /// با پیاده‌سازی IModelBinder، ASP.NET Core را مجبور می‌کنیم به‌جای binder پیش‌فرض، از منطق خودمان استفاده کند.
    /// </summary>
    public class PersonModelBinder : IModelBinder
    {
        /// <summary>
        /// این متد اصلی‌ترین متد binder است.
        /// وقتی درخواستی به اکشنی می‌رسد که [ModelBinder(typeof(PersonModelBinder))] دارد، این متد اجرا می‌شود.
        /// وظیفه‌اش: خواندن مقادیر از درخواست، ساخت آبجکت Person و برگرداندن نتیجه.
        /// </summary>
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            // ۱. یک نمونهٔ خالی از Person می‌سازیم تا مقادیر را داخل آن بریزیم.
            Person person = new Person();

            // ═══════════════════════════════════════════════════════════
            // ۲. FirstName و LastName را می‌خوانیم و در PersonName می‌ریزیم
            // ═══════════════════════════════════════════════════════════
            // ValueProvider یک "دیکشنری" از تمام داده‌های ورودی است.
            // می‌تواند شامل: Form data, Query String, Route Data و... باشد.
            if (bindingContext.ValueProvider.GetValue("FirstName").Length > 0)
            {
                // مقدار FirstName را می‌گیریم.
                person.PersonName = bindingContext.ValueProvider.GetValue("FirstName").FirstValue;

                // اگر LastName هم وجود داشت، به FirstName اضافه‌اش می‌کنیم (با یک فاصله).
                if (bindingContext.ValueProvider.GetValue("LastName").Length > 0)
                {
                    person.PersonName += " " + bindingContext.ValueProvider.GetValue("LastName").FirstValue;
                }
            }

            // ═══════════════════════════════════════════════════════════
            // ۳. Email را از داده‌های ورودی می‌خوانیم
            // ═══════════════════════════════════════════════════════════
            // Length > 0 یعنی "آیا این کلید اصلاً در درخواست وجود دارد؟"
            if (bindingContext.ValueProvider.GetValue("Email").Length > 0)
            {
                person.Email = bindingContext.ValueProvider.GetValue("Email").FirstValue;
            }

            // ═══════════════════════════════════════════════════════════
            // ۴. Phone را می‌خوانیم
            // ═══════════════════════════════════════════════════════════
            if (bindingContext.ValueProvider.GetValue("Phone").Length > 0)
            {
                person.Phone = bindingContext.ValueProvider.GetValue("Phone").FirstValue;
            }

            // ═══════════════════════════════════════════════════════════
            // ۵. Password را می‌خوانیم
            // ═══════════════════════════════════════════════════════════
            if (bindingContext.ValueProvider.GetValue("Password").Length > 0)
            {
                person.Password = bindingContext.ValueProvider.GetValue("Password").FirstValue;
            }

            // ═══════════════════════════════════════════════════════════
            // ۶. ConfirmPassword را می‌خوانیم
            // ═══════════════════════════════════════════════════════════
            if (bindingContext.ValueProvider.GetValue("ConfirmPassword").Length > 0)
            {
                person.ConfirmPassword = bindingContext.ValueProvider.GetValue("ConfirmPassword").FirstValue;
            }

            // ═══════════════════════════════════════════════════════════
            // ۷. Price را می‌خوانیم و به double تبدیل می‌کنیم
            // ═══════════════════════════════════════════════════════════
            // توجه: اگر مقدار قابل تبدیل نباشد، Convert.ToDouble() خطا می‌دهد.
            // در پروژهٔ واقعی بهتر است از double.TryParse استفاده کنید.
            if (bindingContext.ValueProvider.GetValue("Price").Length > 0)
            {
                person.Price = Convert.ToDouble(bindingContext.ValueProvider.GetValue("Price").FirstValue);
            }

            // ═══════════════════════════════════════════════════════════
            // ۸. DateOfBirth را می‌خوانیم و به DateTime تبدیل می‌کنیم
            // ═══════════════════════════════════════════════════════════
            // توجه: اگر فرمت تاریخ اشتباه باشد، Convert.ToDateTime() خطا می‌دهد.
            if (bindingContext.ValueProvider.GetValue("DateOfBirth").Length > 0)
            {
                person.DateOfBirth = Convert.ToDateTime(bindingContext.ValueProvider.GetValue("DateOfBirth").FirstValue);
            }

            // ═══════════════════════════════════════════════════════════
            // ۹. نتیجه را به ASP.NET Core برمی‌گردانیم
            // ═══════════════════════════════════════════════════════════
            // ModelBindingResult.Success: یعنی "با موفقیت ساختم، این آبجکت را تحویل بگیر."
            // اگر binding ناموفق بود (مثلاً دادهٔ ضروری نبود)، می‌توانستیم Failed() برگردانیم.
            bindingContext.Result = ModelBindingResult.Success(person);

            // چون این متد async است، ولی کارمان sync است، یک Task تکمیل‌شده برمی‌گردانیم.
            return Task.CompletedTask;
        }
    }
}