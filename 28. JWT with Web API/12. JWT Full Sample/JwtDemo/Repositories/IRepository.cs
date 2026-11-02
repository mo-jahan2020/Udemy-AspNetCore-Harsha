namespace JwtDemo.Repositories;

/// <summary>
/// قرارداد عمومی Repository — الگوی Repository با اینترفیس جنریک
/// (این بخش جزو پیاده‌سازی JWT نیست).
/// </summary>
public interface IRepository<T> where T : class
{
    Task<IReadOnlyList<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task AddAsync(T entity);
    void Update(T entity);
    void Remove(T entity);
}
