
# 🇮🇷 Part 2 — خلاصهٔ فارسی

<div dir="rtl">

## ۱. اعتبارسنجی از راه دور (Remote Validation) چیست؟

 اعتبارسنجی از راه دور  یک تکنیک در ASP.NET Core MVC است که اجازه می‌دهد  
 یک فیلد مشخص از فرم، در همان لحظه‌ای که کاربر فرم را پر می‌کند، با یک درخواست AJAX به سرور بررسی شود  — 
 بدون Submit کل فرم و بدون رفرش صفحه. پیاده‌سازی آن با اتریبیوت `[Remote]` (از فضای‌نام `Microsoft.AspNetCore.Mvc`) 
 انجام می‌شود که روی یک خاصیت مدل قرار می‌گیرد و مشخص می‌کند برای بررسی صحتِ آن فیلد، کدام اکشن از کدام کنترلر باید صدا زده شود.
 در سمت کلاینت، پلاگین  jQuery Validation Unobtrusive  اتریبیوت‌های `data-val-remote-*` 
 تولیدشده در HTML را می‌خواند و به‌محض خارج‌شدن کاربر از فیلد (رویداد blur)، درخواست AJAX را ارسال می‌کند.

 چرا وجود دارد؟  در فرم ثبت‌نام معمولی، پیام «این ایمیل قبلاً ثبت شده» فقط بعد از Submit کامل فرم و رفرش صفحه دیده می‌شود؛
 اما با `[Remote]` کاربر همان لحظه و دقیقاً زیر همان فیلد پیام را می‌بیند — همان تجربه‌ای که در تمام فرم‌های ثبت‌نام مدرن وجود دارد.

 آنچه این درس انجام می‌دهد:  فیلد `Email` فرم Register با دیتابیس ASP.NET Core Identity چک می‌شود؛ اگر حسابی با آن ایمیل از قبل وجود داشته باشد،
 پیام *"Email is already is use"* بلافاصله زیر فیلد نمایش داده می‌شود.

## ۲. جای این مفهوم در پروژه

سولوشن بر اساس معماری Clean ساخته شده است و این قابلیت دقیقاً روی سه فایل در دو پروژه پیاده شده است:

| # | لایه | فایل | نقش |
|---|------|------|-----|
| ۱ |  Core  | `ContactsManager.Core/DTO/RegisterDTO.cs` | خاصیت `Email` اتریبیوت `[Remote]` را دارد |
| ۲ |  UI  | `ContactsManager.UI/Controllers/AccountController.cs` | اکشن `IsEmailAlreadyRegistered` دیتابیس Identity را چک می‌کند و `Json(true/false)` برمی‌گرداند |
| ۳ |  UI  | `ContactsManager.UI/Views/Account/Register.cshtml` | سه اسکریپت کلاینت را بارگذاری می‌کند که درخواست AJAX را می‌فرستند و پیام خطا را رندر می‌کنند |

(بقیهٔ پروژه‌های سولوشن — `ContactsManager.Infrastructure` و پروژه‌های تست — مربوط به کل برنامه هستند و مختص این درس نیستند.)

## ۳. سه جزء اصلی

### ۳.۱ مدل — `RegisterDTO.cs`

```csharp
[Required(ErrorMessage = "Email can't be blank")]
[EmailAddress(ErrorMessage = "Email should be in a proper email address format")]
[Remote(action: "IsEmailAlreadyRegistered", controller: "Account",
        ErrorMessage = "Email is already is use")]        // ← اعتبارسنجی از راه دور
public string Email { get; set; }
```

اتریبیوت `[Remote]` به MVC می‌گوید: «برای تشخیص معتبربودن `Email`، اکشن `IsEmailAlreadyRegistered` را از کنترلر `Account` صدا بزن؛ اگر نامعتبر بود، `ErrorMessage` را نشان بده.» دقت کنید که این اتریبیوت  در کنار  بقیهٔ اتریبیوت‌ها (`[Required]` و `[EmailAddress]`) قرار دارد — یعنی یک قانون «اضافه» است، نه جایگزین.

### ۳.۲ نقطهٔ پایانی سمت سرور — `AccountController.cs`

```csharp
public async Task<IActionResult> IsEmailAlreadyRegistered(string email)
{
    ApplicationUser? user = await _userManager.FindByEmailAsync(email);

    if (user == null)
        return Json(true);    // ایمیل آزاد است → فیلد معتبر است
    else
        return Json(false);   // ایمیل تکراری است → ErrorMessage نمایش داده می‌شود
}
```

این اکشن با استفاده از `UserManager<ApplicationUser>` (ASP.NET Core Identity) ایمیل را جست‌وجو می‌کند و یک پاسخ JSON کوچک برمی‌گرداند. توجه کنید که برای این بررسی،  کل فرم هرگز  به سرور ارسال نمی‌شود؛ نام پارامتر `email` به‌طور خودکار با نام فیلد تطبیق داده می‌شود و jQuery آن را به‌صورت Query String می‌فرستد.

### ۳.۳ اسکریپت‌های سمت کلاینت — `Register.cshtml`

```html
@section scripts {
    <script src=".../jquery.min.js"></script>
    <script src=".../jquery.validate.min.js"></script>
    <script src=".../jquery.validate.unobtrusive.min.js"></script>
}
```

این سه اسکریپت  اجباری  هستند. بدون آن‌ها هیچ اتفاقی در blur نمی‌افتد؛ چون اتریبیوت `[Remote]` فقط متادیتا (`data-val-remote-url`، `data-val-remote-additionalfields` و…) را در HTML می‌نویسد و باید جاوااسکریپتی باشد که این متادیتا را بخواند، درخواست AJAX را بفرستد و پیام را نمایش دهد.

## ۴. جریان کار گام‌به‌گام

```
۱. کاربر صفحه /Account/Register را باز می‌کند، ایمیلی تایپ می‌کند و از فیلد خارج می‌شود
۲. jquery.validate.unobtrusive اتریبیوت‌های data-val-remote-* را روی input می‌بیند
۳. این درخواست ارسال می‌شود:
   GET /Account/IsEmailAlreadyRegistered?email=user@example.com
۴. AccountController → UserManager.FindByEmailAsync(email)
۵. پاسخ JSON:
        true  → فیلد معتبر است، چیزی نمایش داده نمی‌شود
        false → فیلد نامعتبر است، پیام "Email is already is use" نمایش داده می‌شود
۶. کاربر ایمیل را اصلاح می‌کند → بررسی در تغییر بعدی دوباره اجرا می‌شود
۷. در نهایت فرم به Account/Register ارسال (POST) می‌شود؛
   اعتبارسنجی سمت سرور (ModelState.IsValid) طبق روال دوباره اجرا می‌شود
```

## ۵. قرارداد پاسخ JSON (طبق مستندات مایکروسافت)

| سرور برمی‌گرداند | معنی | کاربر چه می‌بیند؟ |
|---|---|---|
| `Json(true)` | ورودی  معتبر  است | هیچ چیز |
| `Json(false)` یا `null` یا `undefined` | ورودی  نامعتبر  است | پیام `ErrorMessage` اتریبیوت |
| `Json("هر متن دیگری")` | ورودی  نامعتبر  است | همان متن به‌عنوان پیام خطا |

## ۶. نکات مهم

-  `[Remote]` فقط سمت کلاینت است.  طبق مستندات مایکروسافت، این اتریبیوت هنگام POST فرم داخل `ModelState.IsValid`  دوباره اجرا نمی‌شود . محافظت واقعی سمت سرور در این پروژه از خود ASP.NET Core Identity می‌آید: متد `UserManager.CreateAsync(...)` ایمیل تکراری را رد می‌کند و خطای `IdentityError` حاصل در `ViewBag.Errors` نمایش داده می‌شود. برای صحت داده هرگز به `[Remote]` به‌تنهایی تکیه نکنید.
-  جاوااسکریپت باید فعال باشد  و هر سه اسکریپت بارگذاری شوند؛ در غیر این صورت بررسی از راه دور اصلاً انجام نمی‌شود.
-  نقطهٔ پایانی باید برای درخواست AJAX در دسترس باشد.  در این پروژه کل `AccountController` با `[AllowAnonymous]` تزئین شده است، بنابراین حتی کاربران وارد‌نشده هم می‌توانند اکشن اعتبارسنجی را صدا بزنند.
-  بررسی چند فیلد هم‌زمان:  با خاصیت `AdditionalFields` در `[Remote]` می‌توانید فیلدهای دیگری از فرم را هم همراه درخواست بفرستید (مثلاً بررسی ترکیب نام و نام‌خانوادگی) — تمرین تکمیلی طبیعی برای این درس.
- پیام *"Email is already is use"* عیناً متن اصلی مخزن است و در آن غلط تایپی کوچکی وجود دارد که در کپی خودتان بهتر است اصلاحش کنید.

</div>

---

## ▶️ How to run / نحوهٔ اجرا

```bash
dotnet run --project ContactsManager.UI
```

- 🇬🇧 Open `/Account/Register`, register a user, then try registering again with the  same email  — the message appears under the field as soon as you leave it.
- 🇮🇷 به صفحهٔ `/Account/Register` بروید، یک کاربر ثبت‌نام کنید، سپس دوباره با  همان ایمیل  ثبت‌نام کنید — به‌محض خروج از فیلد، پیام خطا زیر آن ظاهر می‌شود.

---

*Summary generated from the repository source — 2025-10-25 / خلاصه بر اساس سورس مخزن تولید شده است*
