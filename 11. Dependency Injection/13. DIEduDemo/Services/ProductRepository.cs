// ============================================================================
//  ProductRepository.cs — پیاده‌سازی IProductRepository با DbContext
// ----------------------------------------------------------------------------
//  نکات آموزشی:
//   - این سرویس به‌صورت Scoped ثبت می‌شود (مثل DbContext).
//   - Constructor Injection: AppDbContext را از Container می‌گیرد.
//   - چون DbContext در یک Request یکسان است، اگر در چند سرویس آن را تزریق کنیم،
//     همه به همان DbContext دسترسی دارند (Change Tracking یکپارچه).
// ============================================================================

using DIEduDemo.Data;
using Microsoft.EntityFrameworkCore;

namespace DIEduDemo.Services;

/// <summary>
/// پیاده‌سازی Repository با EF Core.
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _db;
    private readonly ILogger<ProductRepository> _logger;

    /// <summary>
    /// Constructor Injection: AppDbContext و ILogger را می‌گیرد.
    /// نکته: AppDbContext به‌صورت Scoped ثبت شده، پس در یک Request یک نمونه ثابت داریم.
    /// </summary>
    public ProductRepository(AppDbContext db, ILogger<ProductRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<List<Product>> GetAllAsync()
    {
        _logger.LogInformation("GetAllAsync: گرفتن همه محصولات");
        return await _db.Products
                        .OrderByDescending(p => p.CreatedAt)
                        .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Product?> GetByIdAsync(int id)
    {
        _logger.LogInformation("GetByIdAsync: گرفتن محصول با Id={Id}", id);
        return await _db.Products.FindAsync(id);
    }

    /// <inheritdoc />
    public async Task<Product> AddAsync(string name, decimal price)
    {
        _logger.LogInformation("AddAsync: افزودن محصول {Name}", name);

        var product = new Product
        {
            Name = name,
            Price = price,
            CreatedAt = DateTime.Now
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();  // ذخیره در دیتابیس In-Memory

        return product;
    }

    /// <inheritdoc />
    public async Task<int> CountAsync()
    {
        return await _db.Products.CountAsync();
    }
}
