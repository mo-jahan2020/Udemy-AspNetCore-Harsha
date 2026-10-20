# پروژه ۲۱ — UI Enhancements - Part 1 (ارتقای رابط کاربری - بخش اول)

> منبع: پوشه `20. Filters/21. UI Enhancements - Part 1`
> این پروژه «نگهدارنده» است: کدهای فیلتر دست نمی‌خورند و ظاهر برنامه حرفه‌ای می‌شود

---

## 🎯 مبحث این پروژه چیست؟

این پروژه و پروژه بعد، یک **بازطراحی کامل UI** روی برنامه Contact Manager انجام می‌دهند. منطق فیلترها و Controllerها تقریباً ثابت است؛ تمرکز روی **View ها، Layout و CSS** است. تغییرات اصلی بخش اول:

1. **Layout جدید با Top-Bar حرفه‌ای:** عنوان برنامه با آیکون (Font Awesome)، لینک‌های Register/Login در سمت راست؛
2. **Breadcrumb:** مسیر «Contacts › Persons» بالای صفحه؛
3. **دکمه‌های عملیاتی:** لینک‌های ساده به دکمه‌های استایل‌دار (`button-link button-blue-back`) مثل Create Person / Download as PDF / CSV / Excel؛
4. **جعبه جستجوی بازطراحی‌شده:** dropdown استایل‌دار (`search-dropdown`)، input جستجو (`search-text`)، دکمه Search و لینک «Clear all»؛
5. **پارشیال `_GridColumnHeader.cshtml`:** سرستون‌های مرتب‌شدنی با آیکون‌های جهت (fa-sort-up / fa-sort-down)؛
6. **Font Awesome از CDN** و توسعه StyleSheet.css از ۴۹۴ به ۸۵۰ خط.

## 💡 چرا این پروژه در فصل Filters است؟

دو دلیل: اول، این برنامه مثالِ خود فصل است و برای ادامه مسیر (و جلسات بعدی دوره) باید «ظاهر کاری» داشته باشد. دوم، نکته ظریف آموزشی: بخشی از داده‌هایی که UI مصرف می‌کند (`ViewBag.CurrentSearchBy`، `ViewBag.CurrentSortBy`، ...) همان‌هایی هستند که **فیلترها** می‌سازند — پروژه‌های ۰۳ به بعد. یعنی بازطراحی UI، وابستگی View به فیلترها را هم ملموس می‌کند.

## 🔍 کدهای کلیدی پروژه

**پارشیال سرستون مرتب‌شدنی** — فایل: `Views/Shared/_GridColumnHeader.cshtml`

```cshtml
@if (ViewBag.CurrentSortBy == ViewBag.ColumnName && ViewBag.CurrentSortOrder == SortOrderOptions.ASC.ToString())
{
 <th>
  <a href="~/persons/index?searchBy=@(ViewBag.CurrentSearchBy)&searchString=@(ViewBag.CurrentSearchString)&sortBy=@(ViewBag.ColumnName)&sortOrder=@(nameof(SortOrderOptions.DESC))">@(ViewBag.DisplayName)</a>
  <i class="fa-solid fa-sort-up"></i>
 </th>
}
else if (ViewBag.CurrentSortBy == ViewBag.ColumnName && ViewBag.CurrentSortOrder == SortOrderOptions.DESC.ToString())
{
 <th>
  <a href="...&sortOrder=@(nameof(SortOrderOptions.ASC))">@(ViewBag.DisplayName)</a>
  <i class="fa-solid fa-sort-down"></i>
 </th>
}
else
{
 <th>
  <a href="...&sortOrder=@(nameof(SortOrderOptions.ASC))">@ViewBag.DisplayName</a>
 </th>
}
```

منطق: اگر همین ستون فعلاً مرتب‌سازیِ ASC است، لینک به DESC می‌رود (و آیکون فلش بالا)؛ برعکس؛ در غیر این صورت لینک به ASC. ورودی‌ها با `ViewDataDictionary` از Index پاس می‌شوند:

```cshtml
@await Html.PartialAsync("_GridColumnHeader", new ViewDataDictionary(ViewData) {
    { "ColumnName", nameof(PersonResponse.PersonName) },
    { "DisplayName", "Person Name" },
})
```

**Top-Bar در Layout** — فایل: `Views/Shared/_Layout.cshtml`

```cshtml
<div class="flex" id="top-bar-div">
 <div class="flex-1" id="app-title-div">
  <h1 class="app-title">
   <i class="fa-solid fa-people-simple"></i>
   Contact Manager
  </h1>
 </div>
 <div class="flex-1 pr" id="search-box-div">
  <div class="navbar account-links">
   <ul>
    <li>
     <a asp-controller="Account" asp-action="Register">Register</a>
     <a asp-controller="Account" asp-action="Login">Login</a>
    </li>
   </ul>
  </div>
 </div>
</div>
```

و اتصال Font Awesome در `<head>`:

```cshtml
<link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.1.1/css/all.min.css" ... />
```

## ⚙️ ساختار جدید صفحه Index

```text
┌──────────────────────────────────────────────────────┐
│ Contacts › Persons          [Create] [PDF] [CSV] [Excel] │  ← breadcrumb + دکمه‌ها
│ Persons          [SearchBy ▾] [input] [Search] Clear all │  ← جعبه جستجو
│ ┌──────────────────────────────────────────────────┐ │
│ │ Person Name▲ │ Email │ DOB │ Age │ ... │ Options │ │  ← سرستون مرتب‌شدنی
│ │ ردیف‌ها با Edit / Delete                          │ │
│ └──────────────────────────────────────────────────┘ │
└──────────────────────────────────────────────────────┘
```

## 📊 مفاهیم Razor به‌کاررفته

| مفهوم | نمونه در پروژه |
|---|---|
| Partial View با ورودی | `Html.PartialAsync("_GridColumnHeader", ViewDataDictionary)` |
| `ViewDataDictionary` برای پاس داده به پارشیال | کلیدهای ColumnName / DisplayName |
| Tag Helper لینک | `asp-controller` / `asp-action` / `asp-route-*` |
| CSS Class-based styling | `button-link`، `table-container`، `flex` |
| آیکون‌دهی | کلاس‌های `fa-solid fa-*` از Font Awesome |

## ⚠️ نکات کلیدی و خطاهای رایج

1. **ViewBag نباید null باشد:** پارشیال `_GridColumnHeader` به `ViewBag.CurrentSortBy/Order` وابسته است؛ این مقادیر را `PersonsListActionFilter` می‌سازد. اگر فیلتر حذف شود، صفحات مرتب‌سازی خراب می‌شوند — پیوند پنهان View↔Filter را به یاد داشته باشید (پروژه ۲۲ پیش‌فرض‌دهی را کامل می‌کند).
2. **CDN در محیط بسته:** اگر برنامه اینترانت بدون اینترنت است، Font Awesome باید لوکال شود.
3. **URL ساخت‌شده در پارشیال دستی است:** لینک‌های مرتب‌سازی با href خام ساخته شده‌اند (نه Tag Helper) تا Query String کامل حفظ شود؛ تغییر نام پارامترها در Action یعنی اصلاح این href ها.
4. **CSS خود را گم نکنید:** کلاس‌هایی مثل `search-dropdown` تعریف‌شده در `StyleSheet.css` هستند؛ اگر View را جابه‌جا می‌کنید، فایل CSS را هم همراه ببرید.

## ✅ خلاصه

بخش اول ارتقای UI، ظاهر برنامه را با Layout مدرن (Top-Bar + Register/Login)، Breadcrumb، دکمه‌های استایل‌دار، جعبه جستجوی جدید و سرستون‌های مرتب‌شدنیِ پارشیالی حرفه‌ای کرد. از نظر آموزشی یادآور شد که داده‌های UI این صفحه توسط فیلترها تغذیه می‌شوند — و همین، انگیزه پروژه بعد برای «پیش‌فرض‌دهی» مقادیر ViewData است.
