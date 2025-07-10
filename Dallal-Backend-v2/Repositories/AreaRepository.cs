using Dallal_Backend_v2.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dallal_Backend_v2.Repositories;

public class AreaRepository : Repository<Area>, IAreaRepository
{
    public AreaRepository(DatabaseContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Area>> GetByParentIdAsync(Guid? parentId)
    {
        return await _dbSet
            .Where(a => a.ParentId == parentId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Area>> SearchByNameAsync(string name)
    {
        return await _dbSet
            .Where(a => a.Name.Contains(name) || a.FullName.Contains(name))
            .ToListAsync();
    }

    public async Task<IEnumerable<Area>> GetPaginatedAsync(int page, int pageSize)
    {
        return await _dbSet
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<Area?> GetByFullNameAsync(string fullName)
    {
        return await _dbSet.FirstOrDefaultAsync(a => a.FullName == fullName);
    }

    public async Task<bool> ExistsByFullNameAsync(string fullName)
    {
        return await _dbSet.AnyAsync(a => a.FullName == fullName);
    }
}