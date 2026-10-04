using AsvsLab.Services;
using Microsoft.AspNetCore.Mvc;

namespace AsvsLab.Controllers;

// ═══════════════════════════════════════════════════════════════════
//  فصل V10 — امنیت OAuth و OIDC
//  اجرای واقعی به یک سرور هویت (IdentityServer/Keycloak و...) نیاز دارد؛
//  طبق خواسته کاربر، این فصل با «توضیح متنی + الگوی کد» ارائه می‌شود
// ═══════════════════════════════════════════════════════════════════
public class V10Controller : DemoBaseController
{
    public V10Controller(AsvsRefService asvs, ProgressService progress) : base(asvs, progress) { }

    public IActionResult Index() =>
        SectionIndex(10, "OAuth 2.0 و OpenID Connect: اجرای واقعی به سرور هویت خارجی نیاز دارد؛ همه دموهای این فصل متنی با الگوی کد هستند.");

    public IActionResult StateCsrf() =>
        View("../Shared/DemoResult", TextDemo("v10-state", "V10.1.2,V10.2.1",
            "کلاینت فقط مقادیری را می‌پذیرد که از جریانی آغازشده توسط خودش برگشته‌اند؛ پارامتر state ضد CSRF است.", "V10",
            "سناریوی حمله بدون state:\n" +
            "۱) کاربر به identity provider لاگین است\n" +
            "۲) مهاجم لینک callback جعلی (با کد مجوز «خودش») را برای قربانی می‌فرستد\n" +
            "۳) قربانی ناخواسته حساب مهاجم را به برنامه‌اش متصل می‌کند (Login CSRF)\n\n" +
            "الگوی درست:\n" +
            "var state = RandomNumberGenerator.GetHexString(32);  // CSPRNG\n" +
            "HttpContext.Session.SetString(\"oauth_state\", state);  // ذخیره سمت سرور\n" +
            "// بعد از callback:\n" +
            "if (returnedState != Session.GetString(\"oauth_state\")) return BadRequest();\n\n" +
            "چرا اجرای واقعی نشد؟ نیازمند ثبت client در یک Identity Provider واقعی است."));

    public IActionResult RedirectUri() =>
        View("../Shared/DemoResult", TextDemo("v10-redirect", "V10.4.1",
            "redirect_uri فقط بر اساس فهرست مجاز «مختص هر کلاینت» پذیرفته می‌شود.", "V10",
            "اگر سرور هویت هر redirect_uri را بپذیرد، کد مجوز به دامنه مهاجم می‌رود (سرقت کامل حساب).\n\n" +
            "قواعد بند V10.4.1:\n" +
            "• تطابق دقیق رشته‌ای (نه تطابق پیشوندی!)\n" +
            "• پارامترهای اضافی مثل state در مقایسه نادیده گرفته شوند نه اهمیت داشته باشند\n" +
            "• هر کلاینت فهرست جداگانه دارد\n\n" +
            "نمونه payload خطرناک برای سرور ضعیف:\n" +
            "?redirect_uri=https://app.example.com/callback&next=//evil.example.com\n" +
            "?redirect_uri=https://app.example.com/callback.evil.com   ← تطابق پیشوندی خطرناک است"));

    public IActionResult Pkce() =>
        View("../Shared/DemoResult", TextDemo("v10-pkce", "V10.4.6",
            "با جریان کد، رهگیری کد مجوز باید با PKCE خنثی شود.", "V10",
            "PKCE (تلفظ «pixy») چطور کار می‌کند؟\n" +
            "۱) کلاینت یک code_verifier تصادفی می‌سازد (۴۳ تا ۱۲۸ کاراکتر)\n" +
            "۲) code_challenge = SHA256(verifier) را همراه درخواست مجوز می‌فرستد\n" +
            "۳) هنگام تبادل کد، verifier اصلی را می‌فرستد؛ سرور هش را مقایسه می‌کند\n\n" +
            "مهاجمی که کد را از redirect رهگیری کرده، verifier را ندارد → تبادل رد می‌شود.\n\n" +
            "در .NET:\n" +
            "var verifier = RandomNumberGenerator.GetHexString(64);\n" +
            "var challenge = Base64Url(SHA256.HashData(Encoding.ASCII.GetBytes(verifier)));\n\n" +
            "بندهای تکمیلی: V10.4.5 (چرخش refresh token برای کلاینت عمومی) و V10.4.3 (کوتاه بودن عمر کد مجوز — حداکثر ۱۰ دقیقه)."));

    public IActionResult Replay() =>
        View("../Shared/DemoResult", TextDemo("v10-replay", "V10.3.5",
            "سرور منبع باید از استفاده مجدد/بازپخش توکن‌های مسروقه جلوگیری کند.", "V10",
            "سه راهکار اصلی برای جلوگیری از بازپخش توکن:\n\n" +
            "۱) توکن‌های مقید به فرستنده (Sender-Constrained): DPoP یا mTLS — توکن فقط با کلید صاحبش کار می‌کند (بند V10.4.14)\n" +
            "۲) عمر کوتاه + audience بررسی‌شده (بند V10.3.1 — توکن باید مخصوص همین سرویس باشد)\n" +
            "۳) Deny-list سمت سرور برای توکن‌های باطل‌شده (logout/revoke — بند V10.4.9)\n\n" +
            "بررسی aud در .NET:\n" +
            "new TokenValidationParameters { ValidAudience = \"asvslab-api\", ... }\n" +
            "اگر aud توکن با سرویس فعلی نبود → 401 حتی اگر امضا معتبر باشد."));
}
