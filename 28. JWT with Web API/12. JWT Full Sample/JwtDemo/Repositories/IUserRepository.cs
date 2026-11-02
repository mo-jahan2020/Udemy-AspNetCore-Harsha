using JwtDemo.Models;

namespace JwtDemo.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username);
}
