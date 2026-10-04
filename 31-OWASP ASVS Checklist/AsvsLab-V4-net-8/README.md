# آزمایشگاه OWASP ASVS — راهنمای فارسی

## 🎯 این پروژه چیست؟

یک برنامه آموزشی ASP.NET Core 8 که با زدن هر دکمه، **یک یا چند بند از استاندارد OWASP ASVS 4.0.3** را به‌صورت واقعی اجرا می‌کند و فارسی توضیح می‌دهد. شماره بند مرجع در کامنت کد و در صفحه نتیجه (کادر مشکی) مشخص است.

## ✅ پیش‌نیازها

| پیش‌نیاز | مقدار | لینک |
|---|---|---|
| .NET SDK | 8.0 (LTS) | https://dotnet.microsoft.com/download/dotnet/8.0 |
| Redis | فقط برای دموی «قفل‌سازی با Redis» | توضیح پایین |
| سایر دموها | هیچ سرویس اضافه‌ای لازم نیست (SQLite درون‌حافظه‌ای است) | - |

## 🚀 اجرا

```bash
# ۱) داخل پوشه پروژه
cd AsvsLab

# ۲) بازیابی بسته‌های NuGet و اجرا
dotnet run
```

سپس در مرورگر: `https://localhost:5001` یا آدرسی که کنسول نشان می‌دهد

## 📥 دانلود آفلاین Bootstrap (اگر پوشه wwwroot/lib را ندارید)

فایل‌های Bootstrap 5.3.3 از قبل داخل پروژه (`wwwroot/lib/bootstrap`) هستند؛ اما اگر خواستید خودتان دانلود کنید:

**روش ۱ — با curl (خط فرمان):**
```bash
curl -o wwwroot/lib/bootstrap/dist/css/bootstrap.rtl.min.css https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.rtl.min.css
curl -o wwwroot/lib/bootstrap/dist/js/bootstrap.bundle.min.js https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js
```

**روش ۲ — با PowerShell (ویندوز):**
```powershell
New-Item -ItemType Directory -Force -Path "wwwroot\lib\bootstrap\dist\css","wwwroot\lib\bootstrap\dist\js"
Invoke-WebRequest -Uri "https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.rtl.min.css" -OutFile "wwwroot\lib\bootstrap\dist\css\bootstrap.rtl.min.css"
Invoke-WebRequest -Uri "https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js" -OutFile "wwwroot\lib\bootstrap\dist\js\bootstrap.bundle.min.js"
```

**روش ۳ — با LibMan (ابزار مایکروسافت):**
```bash
dotnet tool install -g Microsoft.Web.LibraryManager.Cli
libman install bootstrap@5.3.3 -p jsdelivr -d wwwroot/lib/bootstrap
```

**روش ۴ — با npm:**
```bash
npm install bootstrap@5.3.3
# سپس از node_modules/bootstrap/dist/css/bootstrap.rtl.min.css کپی کنید
```

> نکته امنیتی: استفاده از فایل محلی (آفلاین) علاوه بر دسترسی بدون اینترنت، بند V14.2.3 (نیاز به SRI برای CDN) را هم بی‌معنا می‌کند و ریسک دستکاری CDN حذف می‌شود.

## 🔴 نصب و اجرای Redis (فقط برای دموی V2.2.1)

**ویندوز — ساده‌ترین روش (WSL):**
```bash
wsl --install          # اگر WSL ندارید
wsl                    # ورود به لینوکس
sudo apt update && sudo apt install redis-server -y
sudo service redis-server start
redis-cli ping         # باید PONG برگرداند
```

**ویندوز — بدون WSL:** نسخه Memurai (سازگار با Redis) را از memurai.com نصب کنید.

**لینوکس:**
```bash
sudo apt install redis-server && sudo systemctl start redis
```

اگر Redis اجرا نباشد، دموی Redis کرش نمی‌کند و پیام راهنمای نصب نشان می‌دهد. بقیه دموها بدون Redis کامل کار می‌کنند.

## 🗂 ساختار پروژه

```
AsvsLab/
├── Program.cs                        ← پیکربندی امن (کوکی، نشست، هدرها، خطا)
├── appsettings.json                  ← تنظیمات + توضیح User Secrets (V6.4.1)
├── Controllers/
│   ├── HomeController.cs             ← داشبورد دکمه‌ها
│   ├── AuthenticationController.cs   ← V2: هش رمز، سیاست رمز، قفل حساب (+Redis)
│   ├── SessionManagementController.cs← V3: کوکی امن، Timeout، ابطال، Fixation
│   ├── AccessControlController.cs    ← V4: Deny by Default، IDOR
│   ├── InputValidationController.cs  ← V5: SQLi واقعی، XSS، Path Traversal، Allow-List
│   ├── CryptographyController.cs     ← V6: AES-GCM، ضدالگوها، CSPRNG، HMAC
│   ├── ErrorLoggingController.cs     ← V7: خطای عمومی، Log Injection، ماسک لاگ
│   ├── DataProtectionController.cs   ← V8: DPAPI، ضدکش، PII
│   └── ConfigurationController.cs    ← V14: هدرها، نسخه سرور، Secrets، SRI
├── Services/                         ← هش‌گر PBKDF2، انبار کاربران، سرویس Redis
├── Models/                           ← مدل نتیجه دموها
├── Views/Shared/                     ← ویوهای مشترک (نتیجه دمو، فرم، فهرست)
└── wwwroot/lib/                      ← Bootstrap RTL + فونت Vazirmatn (آفلاین)
```

## ⚠️ هشدار مهم

این برنامه **فقط برای آموزش** است. کدهای آسیب‌پذیر (کوئری SQL تلفیقی، خروجی بدون انکد و...) صرفاً برای نمایش حمله نوشته شده‌اند و در کنارشان نسخه امن قرار دارد. **هرگز** نسخه آسیب‌پذیر را در پروژه واقعی کپی نکنید.

## 📚 مرجع

- استاندارد کامل: https://owasp.org/www-project-application-security-verification-standard/
- ترجمه فارسی ASVS 4.0: داخل مخزن رسمی OWASP (فایل OWASP Application Security Verification Standard 4.0-fa.pdf)
