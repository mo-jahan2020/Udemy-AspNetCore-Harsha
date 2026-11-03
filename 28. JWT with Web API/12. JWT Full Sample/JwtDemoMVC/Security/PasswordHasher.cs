using BCryptNet = BCrypt.Net.BCrypt;

namespace JwtDemoMVC.Security;

/// <summary>
/// هش کردن رمز عبور با BCrypt (پکیج BCrypt.Net-Next) — جایگزین SHA256 ساده.
///
/// چرا BCrypt از SHA256+Salt بهتر است؟
///  1) <b>کندیِ عمدی (Key Streching)</b>: BCrypt با پارامتر Work Factor هر هش را
///     هزاران بار محاسبه می‌کند (اینجا 2^12 = 4096 دور)؛ حمله‌ی Brute-force روی
///     هش‌های سرقت‌شده را بسیار گران می‌کند. SHA256 یک محاسبه‌ی میلی‌ثانیه‌ای است.
///  2) <b>نمکِ داخلی و خودکار</b>: نمک تصادفی 16 بایتی داخل خودِ رشته‌ی خروجی ذخیره
///     می‌شود (فرمت $2b$12$...)؛ دیگر ستون/فیلد جداگانه‌ی Salt لازم نیست.
///  3) <b>مقایسه‌ی زمان‌ثابت</b>: BCrypt.Verify در برابر Timing Attack امن است.
///  4) <b>استاندارد جاافتاده</b>: از سال 1999 در حال استفاده و در OpenBSD/معتبرترین
///     سیستم‌ها تست‌شده؛ توصیه‌شده توسط OWASP Password Storage Cheat Sheet.
///
/// نتیجه‌ی هر Hash برای یک رمز یکسان هم متفاوت است (به‌خاطر نمک تصادفی) — طبیعی است؛
/// Verify نمک را از داخل رشته‌ی هش استخراج و مقایسه را انجام می‌دهد.
/// (این کلاس جزو پیاده‌سازی JWT نیست و کامنت شماره‌گذاری‌شده ندارد)
/// </summary>
public static class PasswordHasher
{
    // ═══ Work Factor (هزینه‌ی محاسبه) ═══
    // هر واحد، هزینه را دو برابر می‌کند: 2^12 = 4096 دور ≈ ۲۵۰ میلی‌ثانیه روی سخت‌افزار معمولی.
    // ۱۲ توازن خوبی برای برنامه‌ی وب است (OWASP: حداقل 10)؛ در سرورهای ضعیف‌تر 11 و در
    // سرورهای قوی‌تر 13 انتخاب کنید. مقدار داخل خودِ هش ذخیره می‌شود، پس در آینده حتی اگر
    // این ثابت را بالا ببرید، Verify هش‌های قدیمی همچنان درست کار می‌کند و می‌توانید
    // هش‌ها را به‌مرور (در ورود موفق بعدی) با Rehash-upgrade کنید.
    public const int WorkFactor = 12;

    /// <summary>
    /// تولید هش BCrypt از رمز عبور خام — خروجی مثل: $2b$12$KIXl3v... (60 کاراکتر).
    /// نمک تصادفی به‌صورت خودکار تولید و داخل همین رشته جای می‌گیرد؛
    /// دیگر نیازی به نگه‌داری ستون جداگانه‌ی Salt در دیتابیس نیست.
    /// </summary>
    public static string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        return BCryptNet.HashPassword(password, WorkFactor);
    }

    /// <summary>
    /// بررسی رمز عبور خام در برابر هش ذخیره‌شده.
    /// نمک و Work Factor از داخل خودِ رشته‌ی هش خوانده می‌شوند — پارامتر دیگری لازم نیست.
    /// هشِ خراب یا غیرمعتبر هم مثل «رمز اشتباه» رفتار می‌شود (false) نه یک استثنا.
    /// </summary>
    public static bool Verify(string password, string storedHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(storedHash))
            return false;

        try
        {
            // مقایسه به‌صورت زمان‌ثابت انجام می‌شود — در برابر Timing Attack امن است.
            return BCryptNet.Verify(password, storedHash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            // رشته‌ی ذخیره‌شده اصلاً هش BCrypt معتبر نیست (دیتای خراب/ناسازگار)
            return false;
        }
    }
}
