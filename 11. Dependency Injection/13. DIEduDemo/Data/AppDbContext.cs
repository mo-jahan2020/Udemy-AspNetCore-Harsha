// ============================================================================
//  AppDbContext.cs — DbContext نمونه با EF Core In-Memory
// ----------------------------------------------------------------------------
//  نکات آموزشی:
//   - DbContext در ASP.NET Core به‌صورت Scoped ثبت می‌شود (به‌صورت پیش‌فرض).
//   - این یعنی در یک HTTP Request، یک نمونه DbContext به همه‌ی سرویس‌ها داده می‌شود.
//   - در Request بعدی، یک DbContext جدید ساخته می‌شود.
//   - استفاده از In-Memory Database برای آموزش و تست مناسب است؛
//     برای محیط Production باید از SQL Server، PostgreSQL یا SQLite استفاده کنید.
// ============================================================================

using DIEduDemo.Data;
using Microsoft.EntityFrameworkCore;

namespace DIEduDemo.Data;

/// <summary>
/// DbContext اصلی برنامه.
/// این کلاس DbSet<Product> را برای دسترسی به جدول محصولات فراهم می‌کند.
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// Constructor که DbContextOptions را از طریق DI دریافت می‌کند.
    /// این الگوی استاندارد ثبت DbContext در ASP.NET Core است.
    /// </summary>
    /// <param name="options">تنظیمات DbContext (شامل Connection String و Provider)</param>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// مجموعه‌ی محصولات در دیتابیس.
    /// هر DbContext باید حداقل یک DbSet داشته باشد تا کاربردی باشد.
    /// </summary>
    public DbSet<Product> Products => Set<Product>();

    /// <summary>
    /// تنظیمات اضافی مدل (مانند Index، Relation، Seed Data و ...).
    /// </summary>
    /// <remarks>
    /// نکته آموزشی: متدهای مثل ToTable و HasPrecision مختص Providerهای رابطه‌ای
    /// (SQL Server، PostgreSQL) هستند و در In-Memory Provider کار نمی‌کنند.
    /// برای In-Memory از پیکربندی ساده‌تر استفاده می‌کنیم.
    /// </remarks>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // پیکربندی Entity Product
        modelBuilder.Entity<Product>(entity =>
        {
            // محدودیت‌ها روی فیلد Name
            entity.Property(p => p.Name)
                  .IsRequired()
                  .HasMaxLength(200);

            // محدودیت روی فیلد Price
            entity.Property(p => p.Price)
                  .HasConversion<decimal>();  // اطمینان از نوع decimal
        });

        // داده‌ی اولیه (Seed Data)
        // این داده‌ها هنگام ساخت دیتابیس به‌صورت خودکار اضافه می‌شوند.
        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "لپ‌تاپ آموزشی", Price = 25000000m, CreatedAt = new DateTime(2025, 1, 1) },
            new Product { Id = 2, Name = "کتاب ASP.NET Core", Price = 180000m, CreatedAt = new DateTime(2025, 1, 2) },
            new Product { Id = 3, Name = "ماوس بی‌سیم", Price = 350000m, CreatedAt = new DateTime(2025, 1, 3) }
        );

        base.OnModelCreating(modelBuilder);
    }
}
