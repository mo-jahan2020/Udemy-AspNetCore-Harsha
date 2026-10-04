using AsvsAdvancedDemo.Models;
using Microsoft.EntityFrameworkCore;

namespace AsvsAdvancedDemo.Services {
    public interface IUserService {
        Task<bool> IsAdminAsync(string username);
    }

    // بند OWASP ASVS: V4.1.1 - بررسی مجوزها در سمت سرور (نه فقط مخفی کردن دکمه در UI)
    public class UserService : IUserService {
        private readonly AppDbContext _db;
        public UserService(AppDbContext db) { _db = db; }

        public async Task<bool> IsAdminAsync(string username) {
            // در محیط واقعی، این کوئری به دیتابیس SQL Server خارجی ارسال می‌شود
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
            return user?.Role == ""Admin"";
        }
    }
}
