using JwtDemoMVC.Data;
using JwtDemoMVC.Models;
using Microsoft.EntityFrameworkCore;

namespace JwtDemoMVC.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context)
    {
    }

    public Task<User?> GetByUsernameAsync(string username)
        => Context.Users.SingleOrDefaultAsync(u => u.Username == username);
}
