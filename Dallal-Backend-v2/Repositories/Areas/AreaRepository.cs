using System.Linq.Expressions;
using Dallal_Backend_v2.Entities;
using Dallal_Backend_v2.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Dallal_Backend_v2.Repositories.Areas;

public class AreaRepository(DatabaseContext context) : Repository<Area>(context), IAreaRepository
{
    public async override Task<Area> GetAsync(Guid id)
    {
        return await _dbSet
            .Include(a => a.Parent)
            .Include(a => a.Children)
            .FirstOrDefaultAsync(a => a.Id == id) ?? throw new EntityNotFoundException(typeof(Area), id.ToString());
    }

    public async Task<ItemsAndTotalCount<Area>> GetPaginatedAreasWithParentAsync(
        int page,
        int pageSize,
        string? search = null
    )
    {
        return await GetPaginatedAsync(
            page,
            pageSize,
            predicate: area => string.IsNullOrEmpty(search) || ((string)area.Name).Contains(search),
            include: query => query.Include(a => a.Parent)
        );
    }

    public async Task<List<Area>> GetLeafAreasAsync(int page, int pageSize, string? search = null)
    {
        var query = _dbSet.AsQueryable();

        if (!string.IsNullOrEmpty(search))
            query = query.Where(a => a.Name.Contains(search));

        return await query
            .Where(a => !_dbSet.Any(child => child.ParentId == a.Id))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<Dictionary<Guid, Area>> GetAreasByIdsAsync(List<Guid> areaIds)
    {
        return await _dbSet.Where(a => areaIds.Contains(a.Id)).ToDictionaryAsync(a => a.Id);
    }
}
