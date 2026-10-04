using System.Security.Cryptography;

namespace AsvsLab.Services;

// ============================================================================
// ⚙️ بندهای V2.4.1، V2.4.2 و V2.4.3 استاندارد OWASP ASVS 4.0.3:
//  - V2.4.1: رمزهای عبور باید با یک تابع مشتق‌سازی کلید تأییدشده و دارای
//            Salt و ضریب هزینه (Cost Factor) هش شوند.
//  - V2.4.2: طول Salt حداقل ۳۲ بیت و برای هر کاربر منحصربه‌فرد باشد.
//  - V2.4.3: در صورت استفاده از PBKDF2، تعداد تکرار حداقل ۱۰۰,۰۰۰ بار باشد.
//
// در این سرویس از PBKDF2 با الگوریتم HMAC-SHA256، Salt تصادفی ۱۲۸ بیتی و
// ۱۰۰,۰۰۰ تکرار استفاده شده است (با کلاس استاندارد Rfc2898DeriveBytes).
// ============================================================================
public class PasswordHasher
{
    private const int SaltSizeBytes = 16;        // ۱۲۸ بیت Salt (بالاتر از حداقل ۳۲ بیت V2.4.2)
    private const int Iterations = 100_000;      // حداقلِ بند V2.4.3
    private const int HashSizeBytes = 32;        // خروجی ۲۵۶ بیتی

    /// <summary>
    /// تولید یک Salt تصادفی امن (بند V2.4.2) و هش رمز عبور با PBKDF2 (بند V2.4.1)
    /// </summary>
    public (string HashBase64, string SaltBase64) Hash(string password)
    {
        // تولید Salt تصادفی با تولیدکننده عدد تصادفی رمزنگارانه (CSPRNG)
        var salt = RandomNumberGenerator.GetBytes(SaltSizeBytes);

        // PBKDF2: تابع مشتق‌سازی کلید تأییدشده با ضریب هزینه (تعداد تکرار)
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(HashSizeBytes);

        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }

    /// <summary>
    /// بررسی صحت رمز عبور: رمز واردشده با همان Salt و تکرار مجدداً هش شده
    /// و با هش مخزن مقایسه می‌شود (مقاوم در برابر حملات آفلاین)
    /// </summary>
    public bool Verify(string password, string storedHashBase64, string storedSaltBase64)
    {
        var salt = Convert.FromBase64String(storedSaltBase64);
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(HashSizeBytes);
        var storedHash = Convert.FromBase64String(storedHashBase64);

        // مقایسه در زمان ثابت (Constant-Time) برای جلوگیری از Timing Attack
        return CryptographicOperations.FixedTimeEquals(hash, storedHash);
    }
}

// ============================================================================
// ⚙️ سیاست رمز عبور مطابق بندهای V2.1.x استاندارد OWASP ASVS 4.0.3
//  - V2.1.1: حداقل طول ۱۲ کاراکتر (پس از ادغام فاصله‌های متوالی)
//  - V2.1.2: رمز عبور حداقل ۶۴ کاراکتری مجاز و بیش از ۱۲۸ کاراکتر ممنوع
//  - V2.1.3: کوتاه‌سازی (Truncation) رمز عبور ممنوع است
//  - V2.1.4: همه کاراکترهای چاپی یونیکد (حتی ایموجی) مجازند
//  - V2.1.7: بررسی رمز در برابر فهرست رمزهای لو رفته (Breached Passwords)
//  - V2.1.9: قواعد اجباری ترکیب کاراکتر (حرف بزرگ، عدد و...) ممنوع
// ============================================================================
public static class PasswordPolicy
{
    // نمونه کوچکی از «فهرست رمزهای عبور لو رفته» (در پروژه واقعی: فهرست کامل
    // ده‌ها هزار رمز رایج مانند فایل top-10000-passwords یا API مخصوص)
    private static readonly HashSet<string> BreachedPasswords = new(StringComparer.OrdinalIgnoreCase)
    {
        "123456", "password", "qwerty", "111111", "123456789",
        "12345678", "abc123", "password1", "iloveyou", "admin",
        "welcome", "monkey", "dragon", "sunshine", "princess",
        "letmein", "football", "baseball", "master", "666666"
    };

    public record PolicyCheck(string Rule, bool Passed, string Note);

    public static List<PolicyCheck> Evaluate(string password)
    {
        var results = new List<PolicyCheck>();

        // V2.1.3: بدون کوتاه‌سازی؛ فقط فاصله‌های متوالی با یک فاصله جایگزین می‌شوند
        var normalized = System.Text.RegularExpressions.Regex.Replace(password, @" {2,}", " ");

        // V2.1.1: حداقل ۱۲ کاراکتر
        results.Add(new PolicyCheck(
            "حداقل طول ۱۲ کاراکتر (بند V2.1.1)",
            normalized.Length >= 12,
            $"طول پس از نرمال‌سازی فاصله‌ها: {normalized.Length}"));

        // V2.1.2: تا ۶۴ کاراکتر باید مجاز و بیش از ۱۲۸ باید رد شود
        results.Add(new PolicyCheck(
            "رمز ۶۴ کاراکتری مجاز است / بیش از ۱۲۸ رد می‌شود (بند V2.1.2)",
            password.Length <= 128,
            password.Length > 128 ? "رمز بیش از حد مجاز است" : "در محدوده مجاز قرار دارد"));

        // V2.1.7: بررسی در برابر فهرست رمزهای لو رفته
        results.Add(new PolicyCheck(
            "بررسی در برابر فهرست رمزهای لو رفته (بند V2.1.7)",
            !BreachedPasswords.Contains(password),
            BreachedPasswords.Contains(password)
                ? "این رمز در فهرست رمزهای لو رفته جهان قرار دارد!"
                : "در فهرست رمزهای لو رفته مشاهده نشد"));

        // V2.1.9: هیچ قاعده اجباریِ ترکیب کاراکتر (حداقل یک عدد، حرف بزرگ و ...)
        // اعمال نمی‌شود؛ بلکه «طول» + «بررسی لو‌رفتگی» ملاک است
        results.Add(new PolicyCheck(
            "بدون قواعد اجباری ترکیب کاراکتر (بند V2.1.9)",
            true,
            "اجبار حرف بزرگ/عدد/نماد وجود ندارد - مطابق استاندارد ✓"));

        // یک «سنجش‌گر قدرت» ساده برای بند V2.1.8 (قدرت = طول + تنوع کاراکتر)
        int score = 0;
        if (normalized.Length >= 12) score++;
        if (normalized.Length >= 16) score++;
        if (password.Any(char.IsDigit)) score++;
        if (password.Any(char.IsUpper) && password.Any(char.IsLower)) score++;
        if (password.Any(c => !char.IsLetterOrDigit(c))) score++;
        string strength = score switch
        {
            <= 1 => "ضعیف",
            2 => "متوسط",
            3 => "خوب",
            _ => "قوی"
        };
        results.Add(new PolicyCheck(
            "سنجش‌گر قدرت رمز عبور (بند V2.1.8)",
            score >= 3,
            $"قدرت تخمینی: {strength} (امتیاز {score} از 5)"));

        return results;
    }
}
