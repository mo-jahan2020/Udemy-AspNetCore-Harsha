using JwtDemoMVC.Models;
using Microsoft.EntityFrameworkCore;

namespace JwtDemoMVC.Data;

/// <summary>
/// DbContext برنامه — EF Core با رویکرد Code First (این بخش جزو پیاده‌سازی JWT نیست).
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Person> Persons => Set<Person>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    // ویژگی IP AllowList — لیست IPهای مجاز برای استفاده از بخش‌های محافظت‌شده
    public DbSet<AllowedIp> AllowedIps => Set<AllowedIp>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Username).IsUnique();
            entity.Property(u => u.Username).HasMaxLength(50).IsRequired();
            // هش BCrypt همیشه 60 کاراکتر است (فرمت $2b$12$...)؛ طول 200 برای
            // سازگاری با هش‌های طولانی‌ترِ آینده (مثل EnhancedHash) انتخاب شده است.
            entity.Property(u => u.PasswordHash).HasMaxLength(200).IsRequired();
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.Property(r => r.TokenHash).HasMaxLength(200).IsRequired();
            entity.HasOne(r => r.User)
                  .WithMany(u => u.RefreshTokens)
                  .HasForeignKey(r => r.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AllowedIp>(entity =>
        {
            // IP یکتا — دو رکورد برای یک آدرس معنا ندارد (تکرار، تمدید اعتبار همان رکورد است)
            entity.Property(a => a.IpAddress).HasMaxLength(45).IsRequired();
            entity.HasIndex(a => a.IpAddress).IsUnique();
            entity.Property(a => a.Note).HasMaxLength(200);
        });

        base.OnModelCreating(modelBuilder);
    }
}
