using JwtDemo.Models;

namespace JwtDemo.Repositories;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    /// <summary>
    /// جستجوی توکن تمدید بر اساس هش آن — به همراه کاربرِ مالکش
    /// (برای اعتبارسنجی در گام 19 استفاده می‌شود).
    /// </summary>
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash);
}
