using AsvsIntegratedDemo.Models;
using Microsoft.EntityFrameworkCore;

namespace AsvsIntegratedDemo.Services {
    public interface IUserService {
        Task<bool> IsAdminAsync(string username);
    }

    // بند OWASP ASVS: V4.1.1 - بررسی مجوزها در سمت سرور
    public class UserService : IUserService {
        private readonly AppDbContext _db;
        public UserService(AppDbContext db) { _db = db; }

        public async Task<bool> IsAdminAsync(string username) {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
            return user?.Role == "Admin";
        }
    }
}
