using Dallal_Backend_v2.Entities;
using Dallal_Backend_v2.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace Dallal_Backend_v2.Repositories;

public class ListingRepository : Repository<Listing>, IListingRepository
{
    public ListingRepository(DatabaseContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Listing>> GetRecentListingsAsync(int count)
    {
        return await _dbSet
            .Include(l => l.Area)
            .Include(l => l.Broker)
            .ThenInclude(b => b.User)
            .OrderByDescending(l => l.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<Listing>> GetByBrokerIdAsync(Guid brokerId)
    {
        return await _dbSet
            .Include(l => l.Area)
            .Where(l => l.BrokerId == brokerId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Listing>> GetByStatusAsync(ListingStatus status)
    {
        return await _dbSet
            .Include(l => l.Area)
            .Include(l => l.Broker)
            .ThenInclude(b => b.User)
            .Where(l => l.Status == status)
            .ToListAsync();
    }

    public async Task<IEnumerable<Listing>> GetByAreaIdAsync(Guid areaId)
    {
        return await _dbSet
            .Include(l => l.Area)
            .Include(l => l.Broker)
            .ThenInclude(b => b.User)
            .Where(l => l.AreaId == areaId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Listing>> GetByTypeAsync(ListingType type)
    {
        return await _dbSet
            .Include(l => l.Area)
            .Include(l => l.Broker)
            .ThenInclude(b => b.User)
            .Where(l => l.Type == type)
            .ToListAsync();
    }

    public async Task<Listing?> GetDetailedByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(l => l.Area)
            .Include(l => l.Broker)
            .ThenInclude(b => b.User)
            .Include(l => l.Details)
            .ThenInclude(d => d.Definition)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<Listing?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<IEnumerable<Listing>> SearchAsync(string? query, Guid? areaId, ListingType? type, PropertyType? propertyType, decimal? minPrice, decimal? maxPrice)
    {
        var queryable = _dbSet
            .Include(l => l.Area)
            .Include(l => l.Broker)
            .ThenInclude(b => b.User)
            .AsQueryable();

        if (!string.IsNullOrEmpty(query))
            queryable = queryable.Where(l => l.Name.Contains(query) || l.Description.Contains(query));

        if (areaId.HasValue)
            queryable = queryable.Where(l => l.AreaId == areaId.Value);

        if (type.HasValue)
            queryable = queryable.Where(l => l.ListingType == type.Value);

        if (propertyType.HasValue)
            queryable = queryable.Where(l => l.PropertyType == propertyType.Value);

        if (minPrice.HasValue)
            queryable = queryable.Where(l => l.PricePerContract >= minPrice.Value);

        if (maxPrice.HasValue)
            queryable = queryable.Where(l => l.PricePerContract <= maxPrice.Value);

        return await queryable.ToListAsync();
    }

    public async Task<int> GetCountByBrokerIdAsync(Guid brokerId)
    {
        return await _dbSet.CountAsync(l => l.BrokerId == brokerId);
    }

    public async Task<int> GetCountByStatusAsync(ListingStatus status)
    {
        return await _dbSet.CountAsync(l => l.Status == status);
    }

    public async Task UpdateStatusAsync(Guid id, ListingStatus status)
    {
        var listing = await _dbSet.FindAsync(id);
        if (listing != null)
        {
            listing.Status = status;
            await _context.SaveChangesAsync();
        }
    }
}