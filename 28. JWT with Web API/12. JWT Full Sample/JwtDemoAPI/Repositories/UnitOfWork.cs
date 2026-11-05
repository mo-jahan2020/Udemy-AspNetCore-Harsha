using JwtDemoMVC.Data;

namespace JwtDemoMVC.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context,
        IPersonRepository persons,
        IUserRepository users,
        IRefreshTokenRepository refreshTokens,
        IAllowedIpRepository allowedIps)
    {
        _context = context;
        Persons = persons;
        Users = users;
        RefreshTokens = refreshTokens;
        AllowedIps = allowedIps;
    }

    public IPersonRepository Persons { get; }
    public IUserRepository Users { get; }
    public IRefreshTokenRepository RefreshTokens { get; }
    public IAllowedIpRepository AllowedIps { get; }

    public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
}
