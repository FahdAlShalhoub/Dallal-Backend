using System.Linq.Expressions;
using Dallal_Backend_v2.Entities;
using Dallal_Backend_v2.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Dallal_Backend_v2.Repositories;

public class Repository<T>(DatabaseContext context) : IRepository<T>
    where T : BaseEntity
{
    protected readonly DatabaseContext _context = context;
    protected readonly DbSet<T> _dbSet = context.Set<T>();

    public virtual async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }

    public virtual async Task<List<T>> AddRangeAsync(List<T> entities)
    {
        await _dbSet.AddRangeAsync(entities);
        return entities;
    }

    public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate) =>
        await _dbSet.AnyAsync(predicate);

    public virtual async Task<long> CountAsync(Expression<Func<T, bool>>? predicate) =>
        predicate == null ? await _dbSet.LongCountAsync() : await _dbSet.LongCountAsync(predicate);

    public virtual Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public virtual Task DeleteRangeAsync(List<T> entities)
    {
        _dbSet.RemoveRange(entities);
        return Task.CompletedTask;
    }

    public virtual async Task<T?> FindAsync(Guid id) => await _dbSet.FindAsync(id);

    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate) =>
        await _dbSet.FirstOrDefaultAsync(predicate);

    public virtual async Task<T> GetAsync(Guid id) =>
        await FindAsync(id) ?? throw new EntityNotFoundException(typeof(T), id.ToString());

    public virtual async Task<ItemsAndTotalCount<T>> GetPaginatedAsync(
        int page,
        int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        Func<IQueryable<T>, IQueryable<T>>? include = null
    )
    {
        IQueryable<T> query = _dbSet;

        if (predicate != null)
            query = query.Where(predicate);

        if (include != null)
            query = include(query);

        if (orderBy != null)
            query = orderBy(query);
        else
            query = query.OrderByDescending(e => e.CreatedAt);

        var totalCount = await query.LongCountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return new ItemsAndTotalCount<T>(items, totalCount);
    }

    public virtual async Task SaveChangesAsync() => await _context.SaveChangesAsync();

    public virtual Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public virtual Task UpdateRangeAsync(List<T> entities)
    {
        _dbSet.UpdateRange(entities);
        return Task.CompletedTask;
    }
}
