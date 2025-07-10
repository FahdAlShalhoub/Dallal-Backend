using Dallal_Backend_v2.Entities.Enums;
using Dallal_Backend_v2.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Dallal_Backend_v2.Repositories;

public class BrokerRepository : Repository<Broker>, IBrokerRepository
{
    public BrokerRepository(DatabaseContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Broker>> GetByStatusAsync(BrokerStatus status)
    {
        return await _dbSet.Where(b => b.Status == status).ToListAsync();
    }

    public async Task<IEnumerable<Broker>> GetPaginatedAsync(int page, int pageSize, BrokerStatus? status = null)
    {
        var query = _dbSet.AsQueryable();
        
        if (status.HasValue)
            query = query.Where(b => b.Status == status.Value);
            
        return await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetCountByStatusAsync(BrokerStatus status)
    {
        return await _dbSet.CountAsync(b => b.Status == status);
    }

    public async Task<Broker?> GetByUserIdAsync(Guid userId)
    {
        return await _dbSet.FirstOrDefaultAsync(b => b.User.Id == userId);
    }

    public async Task<Broker?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FirstOrDefaultAsync(b => b.Id == id);
    }
}