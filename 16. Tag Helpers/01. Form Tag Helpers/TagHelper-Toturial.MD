Tag Helpers در ASP.NET Core
Tag Helper چیست؟
کدی سمت سرور است که داخل تگ‌های HTML نوشته می‌شود، روی سرور پردازش می‌شودو در HTML نهایی دیده نمی‌شود. جایگزین مدرن Html Helper ها است.

۱️⃣ فعال‌سازی (فایل _ViewImports.cshtml)
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
با این یک خط، همه Tag Helper های استاندارد در تمام View ها فعال می‌شوند.

۲️⃣ ساخت لینک (Anchor Tag Helper)
<a asp-controller="Persons" asp-action="Index">لیست افراد</a>
<!-- ارسال پارامتر با asp-route-{نام} --><a asp-controller="Persons" asp-action="Index"   asp-route-searchBy="PersonName"   asp-route-sortOrder="ASC">مرتب‌سازی</a>
خروجی در HTML: <a href="/Persons/Index?searchBy=PersonName&sortOrder=ASC">

۳️⃣ فرم و ورودی‌ها (Model Binding)
<form asp-controller="Persons" asp-action="Create" method="post">    <label asp-for="PersonName"></label>    <input asp-for="PersonName" class="form-control" />    <span asp-validation-for="PersonName" class="text-danger"></span>    <button type="submit">ثبت</button></form>
asp-for خودکار مقدار، نام، نوع (type) و اعتبارسنجی را به مدل وصل می‌کند.

۴️⃣ لیست کشویی (Select)
<select asp-for="CountryID" asp-items="ViewBag.Countries"        class="form-control">    <option value="">-- انتخاب کنید --</option></select>
۵️⃣ Partial View
<partial name="_GridColumnHeader" model="Model" />
۶️⃣ محیط اجرا (Environment)
<environment include="Development">    <link rel="stylesheet" href="~/css/site.css" /></environment><environment exclude="Development">    <link rel="stylesheet" href="~/css/site.min.css" /></environment>
۷️⃣ نسخه‌دار کردن فایل‌ها (Cache Busting)
<img src="~/images/logo.png" asp-append-version="true" />
خروجی: logo.png?v=AbC123 → با هر تغییر فایل، کش مرورگر باطل می‌شود.

🆚 مقایسه با Html Helper
@* روش قدیمی *@@Html.ActionLink("لیست", "Index", "Persons")<!-- روش جدید (خواناتر و HTML مانا) --><a asp-controller="Persons" asp-action="Index">لیست</a>
✨ مزایا
HTML استاندارد می‌ماند (دیزاینرها هم می‌فهمند)
IntelliSense کامل در Visual Studio / VS Code
انکود خودکار مقادیر (امنیت در برابر XSS)
خطای ناشی از & و شکستن خط در URL وجود ندارد