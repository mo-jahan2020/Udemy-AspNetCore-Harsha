using JwtDemo.Data;
using JwtDemo.Models;
using Microsoft.EntityFrameworkCore;

namespace JwtDemo.Repositories;

public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(AppDbContext context) : base(context)
    {
    }

    public Task<RefreshToken?> GetByTokenHashAsync(string tokenHash)
        => Context.RefreshTokens
            .Include(r => r.User) // برای صدور توکن جدید باید کاربرِ مالک توکن هم بارگذاری شود
            .SingleOrDefaultAsync(r => r.TokenHash == tokenHash);
}
