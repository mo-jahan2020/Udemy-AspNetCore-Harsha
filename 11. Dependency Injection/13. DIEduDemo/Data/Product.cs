// ============================================================================
//  Product.cs — Entity نمونه برای DbContext
// ----------------------------------------------------------------------------
//  این کلاس نشان می‌دهد چطور یک Entity ساده برای EF Core تعریف می‌کنیم.
//  برای آموزش DI، یک Entity ساده کافی است.
// ============================================================================

namespace DIEduDemo.Data;

/// <summary>
/// Entity محصول برای ذخیره‌سازی در دیتابیس In-Memory.
/// </summary>
public class Product
{
    /// <summary>
    /// شناسه یکتای محصول (Primary Key)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// نام محصول
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// قیمت محصول به تومان
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// تاریخ ایجاد رکورد
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
