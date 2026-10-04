using Microsoft.EntityFrameworkCore;

namespace AsvsAdvancedDemo.Models {
    public class User {
        public int Id { get; set; }
        public string Username { get; set; } = """";
        public string PasswordHash { get; set; } = """";
        public string Role { get; set; } = """";
    }

    // بند OWASP ASVS: V4.1.1 - شبیه‌سازی دیتابیس خارجی برای کنترل دسترسی
    public class AppDbContext : DbContext {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
    }
}
