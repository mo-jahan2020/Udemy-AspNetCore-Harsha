using System.Security.Cryptography;
using System.Text;

namespace JwtDemo.Security;

/// <summary>
/// هش کردن رمز عبور با SHA256 + نمک تصادفی.
/// توجه: برای پروژه واقعی از BCrypt یا PBKDF2 استفاده کنید؛ این پیاده‌سازی فقط برای سادگی دمو است.
/// (این کلاس جزو پیاده‌سازی JWT نیست و کامنت شماره‌گذاری‌شده ندارد)
/// </summary>
public static class PasswordHasher
{
    public static (string Hash, string Salt) Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = ComputeHash(password, salt);
        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }

    public static bool Verify(string password, string saltBase64, string expectedHashBase64)
    {
        var salt = Convert.FromBase64String(saltBase64);
        var hash = ComputeHash(password, salt);
        return Convert.ToBase64String(hash) == expectedHashBase64;
    }

    private static byte[] ComputeHash(string password, byte[] salt)
    {
        var input = Encoding.UTF8.GetBytes(Convert.ToBase64String(salt) + password);
        return SHA256.HashData(input);
    }
}
