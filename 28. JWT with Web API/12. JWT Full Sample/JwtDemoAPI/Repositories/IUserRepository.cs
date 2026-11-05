using JwtDemoMVC.Models;

namespace JwtDemoMVC.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username);
}
