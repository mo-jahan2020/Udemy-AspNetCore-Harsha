using Microsoft.AspNetCore.Mvc;

namespace ViewComponentsExample.ViewComponents
{
    // ViewComponent must inherit from ViewComponent class
    // Or [ViewComponent] attribute
    //GridViewComponent is a ViewComponent by default in the Views/Shared/Components/Grid folder
    public class GridViewComponent : ViewComponent
    {
        //  یک تسک اجباری با همین نام
        public async Task<IViewComponentResult> InvokeAsync()
        {
            //return View();//invoked a partial view Views/Shared/Components/Grid/Default.cshtml
            return View("Sample"); //invoked a partial view Views/Shared/Components/Grid/Sample.cshtml
        }
    }
}
//می‌تونه از طریق Dependency Injection به سرویس‌ها و ریپازیتوری‌ها دسترسی داشته باشه (چیزی که Partial View نداره).
//خروجی معمولاً View(model) هست که یک Partial View مخصوص خودش رو رندر می‌کنه.
// کاملاً مستقل عمل میکند ولی ویژگی روتینگ ندارد و قبالیت فرخوانی از طریق URL را ندارد
//چه زمانی از ViewComponent استفاده کنیم؟
//بخشی از UI به دیتای مستقل از خودش نیاز داره (نه از مدل صفحه والد).
//منطق پیچیده‌ای (query، فیلتر، محاسبه) قبل از نمایش لازمه.
//می‌خواید این بخش رو در چند صفحه مختلف بدون تکرار کد استفاده کنید (مثل: sidebar، منوی دسته‌بندی‌ها، ویجت‌های dashboard).

//اگر فقط یک بلوک ساده HTML دارید که دیتاش از بیرون میاد و منطق خاصی نداره، همون Partial View ساده کافیه و نیازی به ViewComponent نیست.
