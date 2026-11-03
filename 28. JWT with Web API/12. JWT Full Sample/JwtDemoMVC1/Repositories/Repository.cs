using JwtDemoMVC.Data;
using Microsoft.EntityFrameworkCore;
namespace JwtDemoMVC.Repositories;

/// <summary>
/// پیاده‌سازی عمومی Repository — همه‌ی Repositoryهای اختصاصی از این کلاس ارث می‌برند.
/// </summary>
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext Context;

    public Repository(AppDbContext context)
    {
        Context = context;
    }

    public virtual async Task<IReadOnlyList<T>> GetAllAsync()
        => await Context.Set<T>().ToListAsync();

    public virtual async Task<T?> GetByIdAsync(int id)
        => await Context.Set<T>().FindAsync(id);

    public virtual async Task AddAsync(T entity)
        => await Context.Set<T>().AddAsync(entity);

    public void Update(T entity) => Context.Set<T>().Update(entity);

    public void Remove(T entity) => Context.Set<T>().Remove(entity);
}
