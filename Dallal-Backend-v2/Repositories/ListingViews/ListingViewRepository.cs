using Dallal_Backend_v2.Entities.Listings;
using Microsoft.EntityFrameworkCore;

namespace Dallal_Backend_v2.Repositories.ListingViews;

public class ListingViewRepository : Repository<ListingView>, IListingViewRepository
{
    public ListingViewRepository(DatabaseContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ListingView>> GetByListingIdAsync(Guid listingId)
    {
        return await _dbSet
            .Include(lv => lv.User)
            .Include(lv => lv.Listing)
            .Where(lv => lv.ListingId == listingId)
            .ToListAsync();
    }

    public async Task<IEnumerable<ListingView>> GetByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Include(lv => lv.User)
            .Include(lv => lv.Listing)
            .Where(lv => lv.UserId == userId)
            .ToListAsync();
    }

    public async Task<int> GetViewCountAsync(Guid listingId)
    {
        return await _dbSet.CountAsync(lv => lv.ListingId == listingId);
    }

    public async Task<int> GetUniqueViewCountAsync(Guid listingId)
    {
        var uniqueUserViews = await _dbSet
            .Where(lv => lv.ListingId == listingId && lv.UserId.HasValue)
            .Select(lv => lv.UserId)
            .Distinct()
            .CountAsync();

        var uniqueDeviceViews = await _dbSet
            .Where(lv => lv.ListingId == listingId && !lv.UserId.HasValue && !string.IsNullOrEmpty(lv.DeviceUuid))
            .Select(lv => lv.DeviceUuid)
            .Distinct()
            .CountAsync();

        return uniqueUserViews + uniqueDeviceViews;
    }

    public async Task<ListingView?> GetExistingViewAsync(Guid listingId, Guid? userId, string? deviceUuid)
    {
        return await _dbSet
            .Where(lv => lv.ListingId == listingId && lv.ViewedAt > DateTime.UtcNow.AddMinutes(-30))
            .Where(lv => (userId.HasValue && lv.UserId == userId) || (!string.IsNullOrEmpty(deviceUuid) && lv.DeviceUuid == deviceUuid))
            .FirstOrDefaultAsync();
    }

    public async Task<Dictionary<Guid, int>> GetViewCountsAsync(List<Guid> listingIds)
    {
        return await _dbSet
            .Where(lv => listingIds.Contains(lv.ListingId))
            .GroupBy(lv => lv.ListingId)
            .ToDictionaryAsync(g => g.Key, g => g.Count());
    }

    public async Task<List<Guid>> GetPopularListingIds(int limit = 10, DateTime? since = null)
    {
        var sinceDate = since ?? DateTime.UtcNow.AddDays(-30);

        return await _dbSet
            .Where(lv => lv.ViewedAt >= sinceDate)
            .GroupBy(lv => lv.ListingId)
            .OrderByDescending(g => g.Count())
            .Take(limit)
            .Select(g => g.Key)
            .ToListAsync();
    }
}