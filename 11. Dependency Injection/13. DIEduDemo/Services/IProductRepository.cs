// ============================================================================
//  IProductRepository.cs — اینترفیس Repository برای دسترسی به محصولات
// ----------------------------------------------------------------------------
//  این الگو (Repository Pattern) نشان می‌دهد چطور یک لایه‌ی انتزاعی روی DbContext
//  می‌سازیم تا کد Business Logic از Entity Framework مستقل باشد.
// ============================================================================

using DIEduDemo.Data;

namespace DIEduDemo.Services;

/// <summary>
/// اینترفیس Repository برای مدیریت محصولات.
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// گرفتن همه‌ی محصولات
    /// </summary>
    Task<List<Product>> GetAllAsync();

    /// <summary>
    /// گرفتن یک محصول بر اساس Id
    /// </summary>
    Task<Product?> GetByIdAsync(int id);

    /// <summary>
    /// افزودن محصول جدید
    /// </summary>
    Task<Product> AddAsync(string name, decimal price);

    /// <summary>
    /// شمارش کل محصولات
    /// </summary>
    Task<int> CountAsync();
}
