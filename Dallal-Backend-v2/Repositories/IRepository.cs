using System.Linq.Expressions;
using Dallal_Backend_v2.Entities;

namespace Dallal_Backend_v2.Repositories;

public interface IRepository<T>
    where T : BaseEntity
{
    Task<T> GetAsync(Guid id);
    Task<T?> FindAsync(Guid id);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    Task<ItemsAndTotalCount<T>> GetPaginatedAsync(
        int page,
        int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        Func<IQueryable<T>, IQueryable<T>>? include = null
    );
    Task<T> AddAsync(T entity);
    Task<List<T>> AddRangeAsync(List<T> entities);
    Task UpdateAsync(T entity);
    Task UpdateRangeAsync(List<T> entities);
    Task DeleteAsync(T entity);
    Task DeleteRangeAsync(List<T> entities);
    Task<long> CountAsync(Expression<Func<T, bool>>? predicate);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
    Task SaveChangesAsync();
}
