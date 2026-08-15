# DownloadMethodsDemo

پروژه آموزشی روش‌های مختلف دانلود فایل در ASP.NET Core (نسخه ۸/۹).

## اجرا

```bash
dotnet run
```

سپس مرورگر را باز کرده و به آدرس زیر بروید:

```
https://localhost:5001
```

## نکته مهم

قبل از اجرا، یک فایل PDF نمونه با نام `sample.pdf` داخل پوشه `wwwroot` قرار دهید.
(در این بسته یک فایل پیش‌فرض کوچک قرار داده شده است.)

## روش‌های پوشش داده شده

1. دانلود مستقیم فایل استاتیک (بدون کنترلر)
2. دانلود از طریق کنترلر MVC:
   - `File(byte[], ...)`
   - `File(Stream, ...)`
   - `PhysicalFile(...)`
   - مسیر مجازی `~/...`
   - `FileContentResult` دستی
   - نوشتن مستقیم روی `Response.Body`
3. دانلود با جاوا اسکریپت (fetch + Blob، Base64، ساخت anchor پویا)
4. دانلود مستقیم با فایل HTML خالص (`wwwroot/direct-download.html`)
5. دانلود با Razor Pages (Handler های GET و POST)
6. دانلود با کامپوننت Blazor Server (`/blazor-download`)

تمام کدها دارای کامنت‌های آموزشی فارسی هستند.
