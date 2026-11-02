namespace JwtDemo.Repositories;

/// <summary>
/// قرارداد Unit of Work — همه‌ی Repositoryها را در یک واحد کاری جمع می‌کند
/// و SaveChanges را متمرکز می‌کند (این بخش جزو پیاده‌سازی JWT نیست).
/// </summary>
public interface IUnitOfWork
{
    IPersonRepository Persons { get; }
    IUserRepository Users { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    Task<int> SaveChangesAsync();
}
