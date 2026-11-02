using JwtDemo.Data;

namespace JwtDemo.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context,
        IPersonRepository persons,
        IUserRepository users,
        IRefreshTokenRepository refreshTokens)
    {
        _context = context;
        Persons = persons;
        Users = users;
        RefreshTokens = refreshTokens;
    }

    public IPersonRepository Persons { get; }
    public IUserRepository Users { get; }
    public IRefreshTokenRepository RefreshTokens { get; }

    public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
}
