using Dallal_Backend_v2.Entities;
using Dallal_Backend_v2.Entities.Enums;
using Dallal_Backend_v2.Entities.Submissions;
using Microsoft.EntityFrameworkCore;

namespace Dallal_Backend_v2.Repositories.Listings;

public class ListingRepository : Repository<Listing>, IListingRepository
{
    public ListingRepository(DatabaseContext context)
        : base(context) { }

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
        return await _dbSet.Include(l => l.Area).Where(l => l.BrokerId == brokerId).ToListAsync();
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

    public async Task<IEnumerable<Listing>> SearchAsync(
        string? query,
        Guid? areaId,
        ListingType? type,
        PropertyType? propertyType,
        decimal? minPrice,
        decimal? maxPrice
    )
    {
        var queryable = _dbSet
            .Include(l => l.Area)
            .Include(l => l.Broker)
            .ThenInclude(b => b.User)
            .AsQueryable();

        if (!string.IsNullOrEmpty(query))
            queryable = queryable.Where(l =>
                l.Name.Contains(query) || l.Description.Contains(query)
            );

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
        }
    }

    public async Task<Listing?> GetListingWithDetailsAsync(Guid id)
    {
        return await _dbSet.Include(l => l.Details).FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<IEnumerable<Listing>> GetBrokerListingsAsync(
        Guid brokerId,
        ListingStatus status,
        int page,
        int pageSize
    )
    {
        return await _dbSet
            .Where(l => l.BrokerId == brokerId && l.Status == status)
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<Listing>> GetBrokerListingsFromSubmissionsAsync(
        Guid brokerId,
        SubmissionStatus status,
        int page,
        int pageSize
    )
    {
        // This method should be implemented in coordination with SubmissionRepository
        // For now, return empty list as this will be handled by the submission repository
        return await Task.FromResult(new List<Listing>());
    }

    public async Task ArchiveListingAsync(Guid id)
    {
        var listing = await _dbSet.FindAsync(id);
        if (listing != null)
        {
            listing.Status = ListingStatus.Archived;
            listing.UpdatedAt = DateTime.UtcNow;
        }
    }

    public async Task UnarchiveListingAsync(Guid id)
    {
        var listing = await _dbSet.FindAsync(id);
        if (listing != null)
        {
            listing.Status = ListingStatus.Active;
            listing.UpdatedAt = DateTime.UtcNow;
        }
    }

    public async Task<bool> ValidateListingOwnershipAsync(Guid listingId, Guid brokerId)
    {
        return await _dbSet.AnyAsync(l => l.Id == listingId && l.BrokerId == brokerId);
    }

    public async Task<IEnumerable<Listing>> GetRecentListingsAsync(int days, int limit)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        return await _dbSet
            .Where(l => l.CreatedAt >= cutoffDate)
            .OrderByDescending(l => l.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<Listing>> GetListingDetailAsync(Guid id, Guid? userId)
    {
        return await _dbSet
            .Include(l => l.Area)
            .Include(l => l.Broker)
            .ThenInclude(b => b.User)
            .Include(l => l.Details)
            .ThenInclude(d => d.Definition)
            .Where(l => l.Id == id)
            .ToListAsync();
    }

    public async Task<IEnumerable<Listing>> GetListingsByAreaAsync(List<Guid> areaIds)
    {
        return await _dbSet.Where(l => areaIds.Contains(l.AreaId)).ToListAsync();
    }

    public async Task<IEnumerable<Listing>> GetListingsByCoordinatesAsync(
        double lat,
        double lng,
        double deltaLat,
        double deltaLng
    )
    {
        return await _dbSet
            .Where(l =>
                l.Location.X >= lat - deltaLat
                && l.Location.X <= lat + deltaLat
                && l.Location.Y >= lng - deltaLng
                && l.Location.Y <= lng + deltaLng
            )
            .ToListAsync();
    }

    public async Task<ItemsAndTotalCount<Listing>> GetBrokerListingsPaginatedAsync(
        Guid brokerId,
        ListingStatus status,
        int page,
        int pageSize
    )
    {
        return await GetPaginatedAsync(
            page,
            pageSize,
            predicate: l => l.BrokerId == brokerId && l.Status == status,
            orderBy: q => q.OrderByDescending(l => l.CreatedAt)
        );
    }

    public async Task<Dictionary<Guid, Listing>> GetListingsByIdsWithDetailsAsync(
        List<Guid> listingIds
    )
    {
        return await _dbSet
            .Where(l => listingIds.Contains(l.Id))
            .Include(l => l.Details)
            .ToDictionaryAsync(l => l.Id);
    }
}
