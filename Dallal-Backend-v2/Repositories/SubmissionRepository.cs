using Dallal_Backend_v2.Entities.Submissions;
using Microsoft.EntityFrameworkCore;

namespace Dallal_Backend_v2.Repositories;

public class SubmissionRepository : Repository<Submission>, ISubmissionRepository
{
    public SubmissionRepository(DatabaseContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Submission>> GetByBrokerIdAsync(int brokerId)
    {
        return await _dbSet
            .Include(s => s.Broker)
            .ThenInclude(b => b.User)
            .Include(s => s.Listing)
            .Where(s => s.BrokerId == brokerId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Submission>> GetByListingIdAsync(int listingId)
    {
        return await _dbSet
            .Include(s => s.Broker)
            .ThenInclude(b => b.User)
            .Include(s => s.Listing)
            .Where(s => s.ListingId == listingId)
            .ToListAsync();
    }

    public async Task<Submission?> GetDetailedByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(s => s.Broker)
            .ThenInclude(b => b.User)
            .Include(s => s.Listing)
            .ThenInclude(l => l.Area)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Submission?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<Submission>> GetPaginatedAsync(int page, int pageSize)
    {
        return await _dbSet
            .Include(s => s.Broker)
            .ThenInclude(b => b.User)
            .Include(s => s.Listing)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _dbSet.CountAsync();
    }
}