using System.Collections.Concurrent;

namespace AsvsLab.Services;

// ============================================================================
// انبار کاربران آزمایشی — فقط برای آموزش و اجرای محلی (در پروژه واقعی:
// دیتابیس + رمز عبور هش‌شده مطابق بند V2.4.1 ذخیره می‌شود)
// ============================================================================
public class DemoUserStore
{
    // کلید: نام کاربری / مقدار: (هش رمز، Salt)
    private readonly ConcurrentDictionary<string, (string Hash, string Salt)> _users = new();

    public ConcurrentDictionary<string, (string Hash, string Salt)> Users => _users;

    public DemoUserStore(PasswordHasher hasher)
    {
        // ثبت دو کاربر نمونه با رمزهای هش‌شده واقعی
        var (h1, s1) = hasher.Hash("Admin@Strong2024");
        _users["admin"] = (h1, s1);

        var (h2, s2) = hasher.Hash("Ali-Pass-2024#Secure");
        _users["ali"] = (h2, s2);
    }

    public (string Hash, string Salt)? Find(string username) =>
        _users.TryGetValue(username, out var v) ? v : null;
}

// ============================================================================
// فاکتورهای ساختگی برای دموی IDOR (بند V4.2.1)
// ============================================================================
public static class InvoiceRepository
{
    public static readonly List<Models.Invoice> Invoices = new()
    {
        new() { Id = 1001, OwnerUserId = 1, OwnerName = "علی رضایی", Description = "خرید اشتراک ماهانه", Amount = 250000 },
        new() { Id = 1002, OwnerUserId = 2, OwnerName = "سارا محمدی", Description = "تمدید لایسنس سالانه", Amount = 1900000 },
        new() { Id = 1003, OwnerUserId = 3, OwnerName = "رضا کریمی", Description = "هزینه پشتیبانی فوری", Amount = 750000 },
    };

    public static Models.Invoice? Find(int id) => Invoices.FirstOrDefault(i => i.Id == id);
}
