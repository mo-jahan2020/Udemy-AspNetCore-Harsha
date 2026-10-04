# 🛡️ آزمایشگاه آموزشی OWASP ASVS 5.0 — نسخه ۲

برنامه ASP.NET Core (C#) با **۶۴ دموی تعاملی** برای آموزش عملی استاندارد **OWASP ASVS نسخه ۵.۰** (۳۴۵ بند رسمی در ۱۷ فصل).
با زدن هر دکمه، یک یا چند بند استاندارد با **مثال واقعی اجرا شده و به فارسی توضیح داده می‌شود**.

## ✨ امکانات نسخه ۲

| امکان | توضیح |
|---|---|
| 🔢 ASVS 5.0 | متن رسمی انگلیسی ۳۴۵ بند در برنامه (فایل `Data/asvs5.json` از مخزن رسمی OWASP تگ `v5.0.0_release`) |
| 🧩 ۱۷ فصل کامل | هر فصل یک کنترلر مجزا: `V1Controller` تا `V17Controller` |
| ❌✅ کد ناامن/امن | هر دمو دو ستون کد دارد: الگوی آسیب‌پذیر در برابر الگوی مورد تأیید ASVS |
| 🔑 ورود واقعی | ثبت‌نام/ورود با کوکی احراز هویت، نقش‌ها (Admin/User)، قفل‌گذاری ۵ تلاش ناموفق |
| 🧪 کوییز | ۲ سوال چهارگزینه‌ای برای هر فصل + ذخیره بهترین نمره |
| 📊 داشبورد پیشرفت + جستجو | دموهای دیده‌شده ✅، درصد پیشرفت هر فصل، جستجو در عنوان/شماره بند |
| 🇮🇷 فارسی RTL | Bootstrap 5.3.3 RTL آفلاین + فونت وزیرمتن |
| 💬 کامنت فارسی | همه کدها کامنت فارسی + شماره بند ASVS مرتبط |

## ⚙️ پیش‌نیازها

- **.NET 10 SDK** — دانلود: https://dotnet.microsoft.com/download/dotnet/10.0
- بررسی نصب بودن:
  ```bash
  dotnet --version    # باید 10.x نمایش دهد
  ```
- **Redis (اختیاری)** — فقط برای دموی «قفل‌گذاری توزیع‌شده» در فصل V6.
  اگر نصب نباشد برنامه کرش نمی‌کند؛ پیام راهنمای فارسی نمایش داده می‌شود:
  ```bash
  # ویندوز (با Docker):     docker run -d -p 6379:6379 redis
  # اوبونتو:                sudo apt install redis-server
  # مک:                     brew install redis && brew services start redis
  ```

## ▶️ اجرا

```bash
cd AsvsLab
dotnet run
# سپس در مرورگر:  https://localhost:5001  یا آدرس نمایش‌داده‌شده در کنسول
```

## 👤 کاربران دمو (برای صفحه ورود)

| کاربر | گذرواژه | نقش | کاربرد |
|---|---|---|---|
| `admin` | `Admin123!` | Admin | دموی کنترل دسترسی سطح تابع (V8) و پنل مدیریت |
| `ali` | `Ali12345!` | User | دموی IDOR (فاکتور 1001 متعلق به admin است!) |

می‌توانید **ثبت‌نام واقعی** هم انجام دهید — سیاست گذرواژه V6.2 (شامل بررسی فهرست لو رفته) واقعاً اعمال می‌شود.

## 🌐 Bootstrap آفلاین

فایل‌های Bootstrap 5.3.3 RTL + فونت وزیرمتن **از قبل داخل `wwwroot/lib` قرار دارند** و برنامه بدون اینترنت کار می‌کند.
اگر خواستید دوباره دانلود کنید:

### ویندوز (PowerShell):
```powershell
.\scripts\download-bootstrap.ps1
```

### لینوکس/مک:
```bash
mkdir -p AsvsLab/wwwroot/lib/bootstrap/dist/css AsvsLab/wwwroot/lib/bootstrap/dist/js AsvsLab/wwwroot/lib/vazirmatn/fonts
curl -L -o AsvsLab/wwwroot/lib/bootstrap/dist/css/bootstrap.rtl.min.css https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.rtl.min.css
curl -L -o AsvsLab/wwwroot/lib/bootstrap/dist/js/bootstrap.bundle.min.js https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js
curl -L -o AsvsLab/wwwroot/lib/vazirmatn/fonts/Vazirmatn-Regular.woff2 https://cdn.jsdelivr.net/gh/rastikerdar/vazirmatn@v33.003/fonts/webfonts/Vazirmatn-Regular.woff2
curl -L -o AsvsLab/wwwroot/lib/vazirmatn/fonts/Vazirmatn-Bold.woff2 https://cdn.jsdelivr.net/gh/rastikerdar/vazirmatn@v33.003/fonts/webfonts/Vazirmatn-Bold.woff2
curl -L -o AsvsLab/wwwroot/lib/vazirmatn/fonts.css https://cdn.jsdelivr.net/gh/rastikerdar/vazirmatn@v33.003/misc/Farsi-Digits/Vazirmatn-CDN-FD.css
```

> 💡 نکته آموزشی: استفاده از Bootstrap آفلاین، خطر «زنجیره تأمین» و CDN را حذف می‌کند
> (بند V15.1.2) و دیگر نیازی به SRI هم نیست. اگر روزی CDN گرفتید، حتماً `integrity="sha384-..."` اضافه کنید.

## 🗺️ نقشه فصل‌ها → کنترلرها (ASVS 5.0)

| فصل | عنوان | دموهای شاخص |
|---|---|---|
| V1 | رمزگذاری و پاک‌سازی | تزریق SQL واقعی (SQLite)، رمزگذاری خروجی، کاننیکالیزاسیون |
| V2 | اعتبارسنجی و منطق تجاری | فهرست مجاز، مبلغ منفی، ارسال تکراری (Idempotency) |
| V3 | امنیت فرانت‌اند | پرچم‌های کوکی، CSP، HSTS، Open Redirect |
| V4 | امنیت API | Content-Type، متدهای HTTP، WebSocket |
| V5 | مدیریت فایل‌ها | Path Traversal واقعی، آپلود امن با Magic Bytes |
| V6 | احراز هویت | PBKDF2، سیاست گذرواژه، قفل‌گذاری Redis/درون‌حافظه |
| V7 | مدیریت نشست | وقفه سشن، تمدید توکن، ابطال خروج |
| V8 | کنترل دسترسی | IDOR واقعی با فاکتور، نقش Admin |
| V9 | توکن‌های خودبسنده | ساخت JWT دست‌ساز، حمله alg=none، دستکاری payload |
| V10 | OAuth و OIDC | state، PKCE، redirect_uri (متنی — نیاز به IdP خارجی) |
| V11 | رمزنگاری | AES-GCM، HMAC، CSPRNG، ممنوعیت MD5 |
| V12 | ارتباطات امن | نسخه‌های TLS، گواهی (متنی — نیاز به گواهی واقعی) |
| V13 | پیکربندی | حذف هدر Server، دیباگ، مدیریت اسرار |
| V14 | محافظت داده | Data Protection واقعی، ضدکش، داده در URL |
| V15 | کدنویسی امن | SBOM، وابستگی‌ها، Mass Assignment |
| V16 | لاگ و خطا | پیام خطای عمومی، ماسک لاگ، تزریق لاگ، UTC |
| V17 | WebRTC | TURN و Signaling (متنی — نیاز به زیرساخت) |

دموهایی که اجرای واقعی‌شان به سرویس/زیرساخت خارجی نیاز دارد (OAuth، TLS واقعی، WebRTC و...)
طبق درخواست شما **به‌صورت متنی + الگوی کد** ارائه شده‌اند و در داشبورد با نشان «متنی» مشخص‌اند.

## 📁 ساختار پروژه

```
AsvsLab/
├── Program.cs                  ← پیکربندی امن با کامنت فارسی + شماره بند
├── Data/asvs5.json             ← متن رسمی ۳۴۵ بند ASVS 5.0
├── Controllers/                ← ۲۰ کنترلر (۱۷ فصل + Home + Account + Quiz)
├── Services/                   ← هش، سیاست گذرواژه، Redis، پیشرفت، کوییز، کاتالوگ
├── Models/                     ← مدل‌های دمو و فرم
├── Views/                      ← RTL، داشبورد جستجو/پیشرفت، DemoResult دوستونه
├── DemoFiles/                  ← فایل‌های واقعی دموی Path Traversal
├── wwwroot/lib/                ← Bootstrap RTL + وزیرمتن (آفلاین)
├── docs/ASVS5-Guide.html       ← راهنمای مرجع فارسی (در مرورگر باز کنید)
└── scripts/download-bootstrap.ps1  ← اسکریپت دانلود Bootstrap آفلاین
```

## 🔒 نکته امنیتی

این پروژه «ابزار آموزش» است و عمداً در هر دمو هم الگوی ناامن را **به‌صورت متن کد** نشان می‌دهد؛
خود برنامه همیشه با الگوی امن پیاده‌سازی شده است. کدهای ناامن هرگز اجرا نمی‌شوند
(مثلاً کوئری تلفیقی SQL فقط برای نمایش خروجی روی دیتابیس درون‌حافظه‌ایِ حاوی داده جعلی اجرا می‌شود).
