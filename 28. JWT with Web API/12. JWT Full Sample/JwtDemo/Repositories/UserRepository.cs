using JwtDemo.Data;
using JwtDemo.Models;
using Microsoft.EntityFrameworkCore;

namespace JwtDemo.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context)
    {
    }

    public Task<User?> GetByUsernameAsync(string username)
        => Context.Users.SingleOrDefaultAsync(u => u.Username == username);
}
