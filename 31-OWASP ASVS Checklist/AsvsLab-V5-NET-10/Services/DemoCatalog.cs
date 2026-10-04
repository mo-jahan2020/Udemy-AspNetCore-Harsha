using AsvsLab.Models;

namespace AsvsLab.Services;

// ═══════════════════════════════════════════════════════════════════
//  کاتالوگ مرکزی همه دموها — برای داشبورد، جستجو و ردیابی پیشرفت
//  ASVS 5.0 دارای ۱۷ فصل الزامات است (V1 تا V17) و همه اینجا پوشش داده شده‌اند
// ═══════════════════════════════════════════════════════════════════
public static class DemoCatalog
{
    public static readonly List<DemoItem> All = new()
    {
        // ── V1 رمزگذاری و پاک‌سازی ──
        new("v1-sqli",   1, "تزریق SQL — پارامتری در برابر تلفیقی", "V1.2.4",          "V1", "SqlInjection", DemoKind.Form),
        new("v1-encode", 1, "رمزگذاری خروجی HTML (ضد XSS)",        "V1.1.2,V1.2.1",   "V1", "OutputEncoding", DemoKind.Live),
        new("v1-canon",  1, "کاننیکالیزاسیون — خطر رمزگشایی دوباره","V1.1.1",          "V1", "Canonicalization", DemoKind.Live),
        new("v1-cmd",    1, "تزریق دستور سیستم‌عامل",               "V1.2.5",          "V1", "CommandInjection", DemoKind.Text),

        // ── V2 اعتبارسنجی و منطق تجاری ──
        new("v2-allowlist", 2, "اعتبارسنجی مثبت با فهرست مجاز",     "V2.1.1,V2.2.1",   "V2", "Allowlist",     DemoKind.Form),
        new("v2-negative",  2, "منطق تجاری — مبلغ منفی و سقف موجودی","V2.3.2",          "V2", "NegativeAmount", DemoKind.Live),
        new("v2-duplicate", 2, "ارسال تکراری و ترتیب عملیات",        "V2.3.1",          "V2", "DuplicateSubmit", DemoKind.Live),
        new("v2-antiauto",  2, "ضد اتوماسیون و سقف فراخوانی",        "V2.4.1",          "V2", "AntiAutomation", DemoKind.Text),

        // ── V3 امنیت فرانت‌اند وب ──
        new("v3-cookie",    3, "پرچم‌های کوکی (Secure/HttpOnly/SameSite)", "V3.3.1,V3.3.2,V3.3.3,V3.3.4", "V3", "CookieFlags",  DemoKind.Live),
        new("v3-csp",       3, "هدر Content-Security-Policy",       "V3.4.3",          "V3", "CspHeader",     DemoKind.Live),
        new("v3-hsts",      3, "هدر HSTS (اجبار HTTPS)",            "V3.4.1",          "V3", "HstsHeader",    DemoKind.Live),
        new("v3-redirect",  3, "Open Redirect — اعتبارسنجی مقصد",   "V3.7.2",          "V3", "OpenRedirect",  DemoKind.Form),
        new("v3-clickjack", 3, "Clickjacking — منع iframe",         "V3.4.3",          "V3", "Clickjacking",  DemoKind.Text),

        // ── V4 امنیت API و سرویس وب ──
        new("v4-content", 4, "درستی هدر Content-Type",               "V4.1.1",          "V4", "ContentType",   DemoKind.Live),
        new("v4-https",   4, "هدایت HTTPS و هدرهای لایه میانی",      "V4.1.2,V4.1.3",   "V4", "HttpsRedirect", DemoKind.Text),
        new("v4-methods", 4, "محدودسازی متدهای HTTP",                "V4.1.4",          "V4", "Methods",       DemoKind.Text),
        new("v4-ws",      4, "امنیت WebSocket (WSS + Origin)",       "V4.4.1,V4.4.2",   "V4", "WebSockets",    DemoKind.Text),

        // ── V5 مدیریت فایل‌ها ──
        new("v5-traversal", 5, "Path Traversal — خواندن فایل با ../", "V5.3.2",         "V5", "PathTraversal", DemoKind.Form),
        new("v5-upload",    5, "آپلود امن — نوع/اندازه/MIME",        "V5.1.1,V5.2.1",  "V5", "SafeUpload",    DemoKind.Form),
        new("v5-public",    5, "فایل آپلودی نباید اجرا شود",         "V5.3.1",         "V5", "PublicFolder",  DemoKind.Text),

        // ── V6 احراز هویت ──
        new("v6-hash",   6, "ذخیره امن گذرواژه — PBKDF2",            "V11.4.2",         "V6", "PasswordHash",  DemoKind.Live),
        new("v6-policy", 6, "سیاست گذرواژه (فرم تعاملی)",            "V6.2.1,V6.2.5,V6.2.9,V6.2.11,V6.2.12", "V6", "PasswordPolicy", DemoKind.Form),
        new("v6-breach", 6, "بررسی گذرواژه لو رفته",                  "V6.2.4",          "V6", "BreachCheck",   DemoKind.Form),
        new("v6-redis",  6, "قفل‌گذاری توزیع‌شده با Redis",           "V6.3.1",          "V6", "RedisLockout",  DemoKind.Live),
        new("v6-memory", 6, "قفل‌گذاری درون‌حافظه‌ای (بدون وابستگی)",  "V6.3.1",          "V6", "MemoryLockout", DemoKind.Live),
        new("v6-login",  6, "ورود واقعی با کوکی احراز هویت",          "V6.2.2,V6.2.3",   "V6", "RealLogin",     DemoKind.Text),

        // ── V7 مدیریت نشست ──
        new("v7-timeout", 7, "وقفه بی‌فعالیتی سشن",                   "V7.1.1,V7.3.1",   "V7", "SessionTimeout", DemoKind.Live),
        new("v7-renew",   7, "تمدید توکن سشن (ضد Session Fixation)",  "V7.2.4",          "V7", "SessionRenewal", DemoKind.Live),
        new("v7-logout",  7, "ابطال کامل سشن در خروج",                "V7.4.1",          "V7", "LogoutInvalidation", DemoKind.Live),
        new("v7-backend", 7, "اعتبارسنجی سشن در بک‌اند",              "V7.2.1",          "V7", "BackendValidation", DemoKind.Text),

        // ── V8 کنترل دسترسی (مجوزدهی) ──
        new("v8-idor",    8, "IDOR — دسترسی به داده دیگری",          "V8.2.2",          "V8", "Idor",          DemoKind.Form),
        new("v8-function",8, "کنترل دسترسی سطح تابع (نقش Admin)",     "V8.2.1",          "V8", "FunctionLevel", DemoKind.Live),
        new("v8-layer",   8, "اعمال مجوز در لایه مورد اعتماد",        "V8.3.1",          "V8", "TrustedLayer",  DemoKind.Text),

        // ── V9 توکن‌های خودبسنده (JWT) ──
        new("v9-jwt",    9, "ساخت و اعتبارسنجی JWT با HS256",         "V9.1.1",          "V9", "JwtCreate",     DemoKind.Live),
        new("v9-tamper", 9, "دستکاری payload و کشف امضا",             "V9.1.1",          "V9", "JwtTamper",     DemoKind.Live),
        new("v9-alg",    9, "حمله alg=none و فهرست مجاز الگوریتم",    "V9.1.2",          "V9", "AlgNone",       DemoKind.Live),
        new("v9-key",    9, "منبع کلید اعتبارسنجی توکن",              "V9.1.3",          "V9", "KeySource",     DemoKind.Text),

        // ── V10 امنیت OAuth و OIDC ──
        new("v10-state",   10, "پارامتر state و CSRF در OAuth",        "V10.1.2,V10.2.1", "V10", "StateCsrf",    DemoKind.Text),
        new("v10-redirect",10, "فهرست مجاز redirect_uri",              "V10.4.1",         "V10", "RedirectUri",  DemoKind.Text),
        new("v10-pkce",    10, "PKCE — محافظت از کد مجوز",             "V10.4.6",         "V10", "Pkce",         DemoKind.Text),
        new("v10-replay",  10, "جلوگیری از بازپخش توکن",               "V10.3.5",         "V10", "Replay",       DemoKind.Text),

        // ── V11 رمزنگاری ──
        new("v11-aes",     11, "رمزنگاری AES-GCM + کشف دستکاری",       "V11.3.2",         "V11", "AesGcm",       DemoKind.Live),
        new("v11-hmac",    11, "امن‌سازی پیام با HMAC",                "V11.4.1",         "V11", "HmacSign",     DemoKind.Live),
        new("v11-random",  11, "تولید عدد تصادفی امن (CSPRNG)",        "V11.5.1",         "V11", "SecureRandom", DemoKind.Live),
        new("v11-md5",     11, "چرا MD5 ممنوع است؟",                   "V11.4.1",         "V11", "Md5Forbidden", DemoKind.Live),
        new("v11-keymgmt", 11, "مدیریت چرخه عمر کلید",                 "V11.1.1,V11.2.1", "V11", "KeyManagement", DemoKind.Text),

        // ── V12 ارتباطات امن ──
        new("v12-tls",     12, "نسخه‌های مجاز TLS",                    "V12.1.1",         "V12", "TlsVersions",  DemoKind.Text),
        new("v12-cipher",  12, "مجموعه‌های رمز (Cipher Suites)",       "V12.1.2",         "V12", "CipherSuites", DemoKind.Text),
        new("v12-cert",    12, "اعتبارسنجی گواهی TLS",                 "V12.3.2",         "V12", "CertValidation", DemoKind.Text),
        new("v12-internal",12, "رمزنگاری ارتباط بین سرویس‌ها",         "V12.3.3",         "V12", "InternalTls",  DemoKind.Text),

        // ── V13 پیکربندی ──
        new("v13-server", 13, "حذف هدر Server (ضد انگشت‌نگاری)",       "V13.4.2",         "V13", "ServerHeader", DemoKind.Live),
        new("v13-debug",  13, "غیرفعال بودن حالت دیباگ",              "V13.4.2",         "V13", "DebugMode",    DemoKind.Live),
        new("v13-vault",  13, "مدیریت اسرار (Vault)",                 "V13.3.1,V13.3.2", "V13", "SecretsVault", DemoKind.Text),
        new("v13-rotate", 13, "چرخش اسرار امنیتی",                    "V13.1.4",         "V13", "SecretRotation", DemoKind.Text),

        // ── V14 محافظت از داده‌ها ──
        new("v14-class", 14, "طبقه‌بندی داده‌های حساس",               "V14.1.1",         "V14", "DataClassification", DemoKind.Text),
        new("v14-url",   14, "داده حساس در URL ممنوع",               "V14.2.1",         "V14", "SensitiveInUrl", DemoKind.Text),
        new("v14-dp",    14, "رمزنگاری در سکون با Data Protection",  "V14.2.4",         "V14", "DpAtRest",     DemoKind.Live),
        new("v14-cache", 14, "هدرهای ضدکش (Cache-Control)",          "V14.3.2",         "V14", "AntiCaching",  DemoKind.Live),
        new("v14-clear", 14, "پاک‌سازی داده پس از پایان نشست",       "V14.3.1",         "V14", "ClearSiteData", DemoKind.Text),

        // ── V15 کدنویسی امن و معماری ──
        new("v15-sbom",  15, "فهرست اجزای نرم‌افزار (SBOM)",          "V15.1.2",         "V15", "Sbom",         DemoKind.Text),
        new("v15-deps",  15, "وابستگی‌های آسیب‌پذیر",                 "V15.1.1,V15.2.1", "V15", "Dependencies", DemoKind.Text),
        new("v15-mass",  15, "حمله Mass Assignment",                  "V15.3.3",         "V15", "MassAssignment", DemoKind.Text),
        new("v15-proto", 15, "آلودگی Prototype در جاوااسکریپت",       "V15.3.6",         "V15", "PrototypePollution", DemoKind.Text),

        // ── V16 لاگ امنیتی و مدیریت خطا ──
        new("v16-generic", 16, "پیام خطای عمومی (بدون جزئیات داخلی)", "V16.5.1",         "V16", "GenericError", DemoKind.Live),
        new("v16-safe",    16, "لاگ بدون داده حساس (ماسک‌سازی)",      "V16.2.5",         "V16", "SafeLogging",  DemoKind.Live),
        new("v16-inject",  16, "ضد تزریق لاگ (CRLF)",                 "V16.4.1",         "V16", "LogInjection", DemoKind.Live),
        new("v16-utc",     16, "زمان UTC در لاگ‌ها",                   "V16.2.2",         "V16", "UtcTimestamp", DemoKind.Live),
        new("v16-authz",   16, "ثبت لاگ تلاش‌های ناموفق مجوز",         "V16.3.2",         "V16", "AuthzLogging", DemoKind.Text),

        // ── V17 امنیت WebRTC ──
        new("v17-overview", 17, "مرور امنیت WebRTC (TURN/DTLS)",       "V17.1.1",         "V17", "Overview",     DemoKind.Text),
        new("v17-signal",   17, "سطح‌بندی سرور Signaling",              "V17.3.1",         "V17", "Signaling",    DemoKind.Text),
    };

    public static List<DemoItem> ForChapter(int ch) => All.Where(d => d.Chapter == ch).ToList();
    public static DemoItem? Find(string controller, string action) =>
        All.FirstOrDefault(d => d.Controller == controller && d.Action == action);
    public static DemoItem? Find(string id) => All.FirstOrDefault(d => d.Id == id);
    public static int Total => All.Count;
}
