using System.Security.Cryptography;

namespace AsvsLab.Services;

// ═══════════════════════════════════════════════════════════════════
//  بند V11.4.2 — گذرواژه‌ها باید با یک تابع مشتق کلید (KDF) تأییدشده
//  و «مصرف‌کننده منابع» ذخیره شوند.
//  نکته مهم نسخه 5.0: الزام ذخیره امن گذرواژه از فصل احراز هویت به
//  فصل رمزنگاری (V11) منتقل شده است — در نسخه 4.0.3 بند V2.4.1 بود.
//
//  پیاده‌سازی: PBKDF2 با SHA-256 و ۱۰۰,۰۰۰ تکرار + نمک تصادفی ۱۶۰ بیتی.
//  در تولید واقعی: Argon2id یا PBKDF2 با تکرار بیشتر (OWASP: ۶۰۰,۰۰۰) توصیه می‌شود.
// ═══════════════════════════════════════════════════════════════════
public class PasswordHasher
{
    private const int Iterations = 100_000; // بند V11.4.2 — پارامترهای KDF بر اساس راهنمای روز

    /// <summary>ساخت هش PBKDF2 با نمک تصادفی — خروجی: PBKDF2$تکرار$نمک$هش</summary>
    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(20); // بند V11.5.1 — نمک با CSPRNG
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, 32);
        return $"PBKDF2${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    /// <summary>بررسی گذرواژه با مقایسه زمان‌ثابت (ضد Timing Attack)</summary>
    public bool Verify(string password, string stored)
    {
        var parts = stored.Split('$');
        if (parts.Length != 4 || parts[0] != "PBKDF2") return false;
        var salt = Convert.FromBase64String(parts[2]);
        var expected = Convert.FromBase64String(parts[3]);
        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt,
            int.Parse(parts[1]), HashAlgorithmName.SHA256, expected.Length);
        return CryptographicOperations.FixedTimeEquals(actual, expected); // بند V11 — مقایسه امن
    }
}

// ═══════════════════════════════════════════════════════════════════
//  سیاست گذرواژه مطابق بندهای V6.2 (فصل احراز هویت در ASVS 5.0):
//    V6.2.1 — حداقل ۸ کاراکتر (توصیه: ۱۵)
//    V6.2.5 — هیچ محدودیتی روی «نوع کاراکترها» نباید باشد
//    V6.2.9 — گذرواژه تا حداقل ۶۴ کاراکتر باید پذیرفته شود
//    V6.2.11 — کلمات وابسته به زمینه (نام سایت، نام کاربری...) ممنوع
//    V6.2.12 — بررسی در برابر فهرست گذرواژه‌های لو رفته
// ═══════════════════════════════════════════════════════════════════
public record PolicyCheck(string Ref, string FaTitle, bool Passed, string Detail);

public static class PasswordPolicy
{
    // گذرواژه‌های رایج لو رفته (بند V6.2.12 — نسخه کوچک برای دمو؛ در واقعیت: HaveIBeenPwned)
    public static readonly HashSet<string> BreachedList = new(StringComparer.OrdinalIgnoreCase)
    {
        "123456", "password", "12345678", "qwerty", "abc123", "111111",
        "123456789", "1234567", "iloveyou", "aa123456", "admin", "letmein"
    };

    // کلمات وابسته به زمینه این برنامه (بند V6.2.11)
    public static readonly HashSet<string> ContextWords = new(StringComparer.OrdinalIgnoreCase)
    { "asvslab", "asvs", "owasp", "admin", "administrator", "security" };

    public static List<PolicyCheck> Evaluate(string password)
    {
        var checks = new List<PolicyCheck>
        {
            new("V6.2.1", "حداقل طول گذرواژه",
                password.Length >= 8,
                password.Length >= 8
                    ? $"طول {password.Length} کاراکتر — حداقل ۸ رعایت شده (توصیه استاندارد: ۱۵)"
                    : $"طول {password.Length} کاراکتر — کمتر از حداقل ۸ است"),

            new("V6.2.5", "عدم محدودیت نوع کاراکتر",
                true,
                "سیاست درست: هیچ الزامی برای «علائم خاص/حروف بزرگ» وجود ندارد (بند صریح ASVS 5.0)"),

            new("V6.2.9", "پذیرش گذرواژه بلند (۶۴ کاراکتر)",
                password.Length <= 64,
                password.Length <= 64
                    ? "طول مجاز است؛ سامانه باید تا ۶۴ کاراکتر را بپذیرد"
                    : "بیش از ۶۴ کاراکتر — سامانه واقعی باید تا ۶۴ را بپذیرد"),

            new("V6.2.11", "کلمات وابسته به زمینه",
                !ContextWords.Any(w => password.Contains(w, StringComparison.OrdinalIgnoreCase)),
                ContextWords.Any(w => password.Contains(w, StringComparison.OrdinalIgnoreCase))
                    ? "گذرواژه شامل کلمات ممنوعه مرتبط با این برنامه است (مثل asvslab/admin)"
                    : "شامل هیچ کلمه وابسته به زمینه (نام سایت/کاربری) نیست"),

            new("V6.2.12", "بررسی فهرست گذرواژه‌های لو رفته",
                !BreachedList.Contains(password),
                BreachedList.Contains(password)
                    ? "این گذرواژه در فهرست رایج‌ترین گذرواژه‌های لو رفته جهان است!"
                    : "در فهرست گذرواژه‌های لو رفته یافت نشد"),
        };
        return checks;
    }
}
