# پروژه ۲۲ — UI Enhancements - Part 2 (ارتقای رابط کاربری - بخش دوم)

> منبع: پوشه `20. Filters/22. UI Enhancements - Part 2`
> پیش‌نیاز: پروژه ۲۱ (بازطراحی بخش اول UI)

---

## 🎯 مبحث این پروژه چیست؟

بخش دوم ارتقای UI دو کار انجام می‌دهد: **تکمیل بازطراحی در همه صفحات** و **اصلاح یک باگ واقعی در فیلتر** (پیش‌فرض‌دهی مقادیر ViewData). تغییرات اصلی نسبت به پروژه ۲۱:

1. **Breadcrumb در همه صفحات  صفحات Create / Edit / Delete / Upload Countries حالا مسیر «Contacts › Persons › Create» و مانند آن را نشان می‌دهند؛
2. **فرم‌ها داخل `form-container`  چیدمان بهتر فرم‌های Create/Edit؛
3. **دکمه‌ای شدن Edit/Delete در جدول  `button-link button-blue-back` برای Edit و `button-link button-red-back` برای Delete؛
4. **Nav اصلی در Layout  لینک‌های Persons و Upload Countries زیر Top-Bar؛
5. **(مهم — سمت فیلتر)  `PersonsListActionFilter` حالا وقتی پارامترهای sortBy/sortOrder وجود ندارند، مقدار **پیش‌فرض** را در ViewData می‌گذارد.

## 💡 چرا پیش‌فرض‌دهی در فیلتر مهم است؟

در پروژه ۰۳ دیدیم فیلتر فقط وقتی کلید در پارامترها بود، `ViewData["CurrentSortBy"]` را ست می‌کرد. اما اگر کاربر صفحه را بدون `sortBy` باز کند (URL ساده `persons/index`)، آن ViewData مقدار نمی‌گیرد → پارشیال `_GridColumnHeader` که به `ViewBag.CurrentSortBy` مقایسه می‌کند با null سر و کار می‌یابد → هیچ سرستونی «فعال» نشان داده نمی‌شود و ریسک خطا هست. راه‌حل: در فیلتر، شاخه `else` اضافه شود.

## 🔍 کد نمونه — تغییر فیلتر

فایل: `CRUDExample/Filters/ActionFilters/PersonsListActionFilter.cs` (بخش تازه)

```csharp
using ServiceContracts.Enums;   // ← using جدید

// داخل OnActionExecuted:
if (parameters.ContainsKey("sortBy"))
{
 personsController.ViewData["CurrentSortBy"] = Convert.ToString(parameters["sortBy"]);
}
else
{
 personsController.ViewData["CurrentSortBy"] = nameof(PersonResponse.PersonName);
}

if (parameters.ContainsKey("sortOrder"))
{
 personsController.ViewData["CurrentSortOrder"] = Convert.ToString(parameters["sortOrder"]);
}
else
{
 personsController.ViewData["CurrentSortOrder"] = nameof(SortOrderOptions.ASC);
}
```

حالا در هر حالتی، View مقادیر معتبر `PersonName` و `ASC` را می‌بیند — همان پیش‌فرض‌های امضای Action (`sortBy = nameof(PersonResponse.PersonName)`, `sortOrder = SortOrderOptions.ASC`). اکنون **View و فیلتر همیشه هم‌خوان‌اند**.

## 🔍 کد نمونه — Breadcrumb در صفحات

فایل: `Views/Persons/Create.cshtml` (الگوی مشترک همه صفحات)

```cshtml
<!-- breadcrumb starts -->
<div class="flex">
 <div class="flex-1">
  <div class="text-grey">
   <span>Contacts</span>
   <i class="fa-solid fa-caret-right"></i>
   <span class="text-grey">Persons</span>
   <i class="fa-solid fa-caret-right"></i>
   <span class="text-grey">Create</span>
  </div>
 </div>
</div>
<!-- breadcrumb ends -->

<div class="form-container">
 <a asp-controller="Persons" asp-action="Index" class="link-hover">Back to Persons List</a>
 <h2 class="text-large">Create Person</h2>
 ...
```

و در جدول صفحه Index، دکمه‌های ردیف:

```cshtml
<a asp-controller="Persons" asp-action="Edit" asp-route-personID="@person.PersonID"
   class="button-link button-blue-back">Edit</a>
<a asp-controller="Persons" asp-action="Delete" asp-route-personID="@person.PersonID"
   class="button-link button-red-back">Delete</a>
```

## ⚙️ خلاصه تغییرات نسبت به پروژه ۲۱

| فایل | تغییر |
|---|---|
| `Filters/ActionFilters/PersonsListActionFilter.cs` | شاخه else برای sortBy/sortOrder → پیش‌فرض‌دهی ViewData |
| `Views/Persons/Index.cshtml` | دکمه‌ای شدن Edit/Delete؛ نظرسازی (comment) breadcrumb |
| `Views/Persons/Create.cshtml` / `Edit.cshtml` / `Delete.cshtml` | Breadcrumb + `form-container` / `center-box` |
| `Views/Countries/UploadFromExcel.cshtml` | Breadcrumb «Contacts › Countries › Upload» |
| `_Layout.cshtml` | بدون تغییر (از پروژه ۲۱) |

## 🧪 رفتار برنامه در اجرا

- باز کردن `persons/index` (بدون هیچ Query String): سرستون PersonName به‌عنوان ستون مرتب فعال (ASC) دیده می‌شود — چون فیلتر پیش‌فرض‌ها را ست کرده است؛
- ورود به `persons/create`: مسیر «Contacts › Persons › Create» بالای فرم؛
- ردیف‌های جدول: Edit آبی و Delete قرمز.

## 📊 مفاهیم کلیدی

| مفهوم | یادآوری |
|---|---|
| Defensive Coding در فیلتر | هر کلید اختیاری، شاخه else دارد |
| هم‌خوانی فیلتر↔View | پیش‌فرض‌های فیلتر = پیش‌فرض‌های امضای Action |
| یکسان‌سازی الگوی صفحات | Breadcrumb + Container در همه Viewها |
| معنای رنگ در UI | آبی = عملیات امن، قرمز = عملیات مخرب |

## ⚠️ نکات کلیدی و خطاهای رایج

1. **منبع حقیقت پیش‌فرض‌ها را یکی نگه دارید  پیش‌فرض‌ها در سه جا ظاهر شده‌اند (امضای Action، فیلتر، View). اگر روزی پیش‌فرض sortBy تغییر کند، هر سه را هماهنگ کنید — در بازسازی‌های بزرگ‌تر این منطق به یک جای مشترک منتقل می‌شود.
2. **Breadcrumb دست‌ساز است  کپی‌پیست بین صفحات؛ اگر پروژه بزرگ شود، راه‌حل (Partial با پارامتر یا ViewComponent) جایگزین می‌شود — ابزارهایی که در فصل‌های Partial Views و View Components یاد گرفته‌اید.
3. **استایل ردیف‌های جدول از CSS می‌آید  کلاس‌های دکمه در `StyleSheet.css` تعریف شده‌اند؛ بدون آن فایل، صفحه به حالت لینک‌های خام برمی‌گردد.
4. **تست هر دو مسیر  بعد از این تغییر، هم `persons/index` ساده و هم نسخه‌های با Query String را امتحان کنید تا پیش‌فرض‌ها را ببینید.

## ✅ خلاصه

بخش دوم ارتقای UI، بازطراحی را به همه صفحات کشاند (Breadcrumb، فرم‌ها، دکمه‌های Edit/Delete) و مهم‌تر از آن، در `PersonsListActionFilter` پیش‌فرض‌های sortBy/sortOrder را برای ViewData تضمین کرد تا View همیشه با مقادیر معتبر کار کند. این پروژه نمونه خوبی است از این‌که «بهبود UI» اغلب یک اصلاح کوچک سمت فیلتر هم می‌طلبد.
