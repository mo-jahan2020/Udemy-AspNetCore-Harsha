using Microsoft.EntityFrameworkCore;
using TextToSqlApi.Models.Entities;

namespace TextToSqlApi.Data;

/// <summary>
/// DbContext مخصوص دیتابیس سشن (SQL Server Express) که تاریخچه
/// نشست‌ها و پیام‌های کاربران را نگه‌داری می‌کند.
/// </summary>
public class SessionDbContext : DbContext
{
    /// <summary>سازنده DbContext که گزینه‌ها از طریق DI تزریق می‌شوند.</summary>
    public SessionDbContext(DbContextOptions<SessionDbContext> options) : base(options)
    {
    }

    /// <summary>مجموعه نشست‌ها.</summary>
    public DbSet<Session> Sessions => Set<Session>();

    /// <summary>مجموعه پیام‌های نشست‌ها.</summary>
    public DbSet<SessionMessage> SessionMessages => Set<SessionMessage>();

    /// <summary>پیکربندی مدل‌ها و روابط بین موجودیت‌ها.</summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // پیکربندی جدول Sessions
        modelBuilder.Entity<Session>(entity =>
        {
            entity.ToTable("Sessions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("Id");
            entity.Property(e => e.UserName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Title).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime2");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime2");

            entity.HasMany(e => e.Messages)
                  .WithOne(e => e.Session!)
                  .HasForeignKey(e => e.SessionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // پیکربندی جدول SessionMessages
        modelBuilder.Entity<SessionMessage>(entity =>
        {
            entity.ToTable("SessionMessages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("Id");
            entity.Property(e => e.SessionId).IsRequired();
            entity.Property(e => e.Role).HasConversion<int>();
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.GeneratedSql).HasMaxLength(int.MaxValue);
            entity.Property(e => e.ModelName).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime2");

            entity.HasIndex(e => e.SessionId);
        });
    }
}
