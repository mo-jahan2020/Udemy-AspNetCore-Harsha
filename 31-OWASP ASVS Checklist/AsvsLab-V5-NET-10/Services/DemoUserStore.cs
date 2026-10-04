using System.Collections.Concurrent;
using AsvsLab.Models;

namespace AsvsLab.Services;

// ═══════════════════════════════════════════════════════════════════
//  انبار کاربران و داده دمو (جایگزین دیتابیس واقعی برای آموزش)
//  کاربران با نقش (بند V8.2.1 — کنترل دسترسی سطح تابع) و فاکتورها
//  (بند V8.2.2 — کنترل دسترسی سطح داده / مقابله با IDOR)
//  ثبت‌نام واقعی: گذرواژه با PBKDF2 هش می‌شود (بند V11.4.2)
// ═══════════════════════════════════════════════════════════════════
public record DemoUser(int Id, string Username, string PasswordHash, string FullName, string Role);

public record Invoice(int Id, int OwnerUserId, string OwnerName, string Description, decimal Amount);

public class DemoUserStore
{
    private readonly ConcurrentDictionary<int, DemoUser> _users = new();
    private readonly ConcurrentDictionary<string, DemoUser> _byName = new(StringComparer.OrdinalIgnoreCase);
    private int _nextId = 100;

    public List<Invoice> Invoices { get; } = new()
    {
        new(1001, 1, "مدیر آزمایشی", "خرید سرور ابری — شرکت نوآور", 25_000_000m),
        new(1002, 2, "علی رضایی", "اشتراک سالانه پلن حرفه‌ای", 4_800_000m),
        new(1003, 2, "علی رضایی", "خرید دامنه و SSL", 1_200_000m),
    };

    public DemoUserStore(PasswordHasher hasher)
    {
        // کاربران از پیش ساخته‌شده برای دمو — روی صفحه ورود نمایش داده می‌شوند
        var admin = new DemoUser(1, "admin", hasher.Hash("Admin123!"), "مدیر آزمایشی", "Admin");
        var ali   = new DemoUser(2, "ali",   hasher.Hash("Ali12345!"),  "علی رضایی",    "User");
        _users[admin.Id] = admin; _byName[admin.Username] = admin;
        _users[ali.Id] = ali;     _byName[ali.Username] = ali;
    }

    public DemoUser? FindByName(string username) =>
        _byName.TryGetValue(username.Trim(), out var u) ? u : null;

    public DemoUser? FindById(int id) => _users.TryGetValue(id, out var u) ? u : null;

    /// <summary>ثبت‌نام واقعی — هش PBKDF2 ذخیره می‌شود، نه متن آشکار (بند V11.4.2)</summary>
    public DemoUser Register(string username, string password, string fullName, PasswordHasher hasher)
    {
        var user = new DemoUser(_nextId++, username.Trim(), hasher.Hash(password), fullName.Trim(), "User");
        _users[user.Id] = user;
        _byName[user.Username] = user;
        return user;
    }

    public bool Exists(string username) => _byName.ContainsKey(username.Trim());
    public int TotalUsers() => _users.Count;

    // ── دسترسی سطح داده برای دموی IDOR ──
    public List<Invoice> InvoicesOf(int userId) => Invoices.Where(i => i.OwnerUserId == userId).ToList();
    public Invoice? FindInvoice(int id) => Invoices.FirstOrDefault(i => i.Id == id);
}
