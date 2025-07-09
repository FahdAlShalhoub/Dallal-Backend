using Dallal_Backend_v2.Entities.Listings;
using Microsoft.EntityFrameworkCore;

namespace Dallal_Backend_v2.Services;

public class ListingViewService(DatabaseContext _context)
{
    public async Task<ListingView> AddViewAsync(
        Guid listingId,
        Guid? userId = null,
        string? deviceUuid = null,
        string? ipAddress = null,
        string? userAgent = null
    )
    {
        // Check if listing exists
        var listingExists = await _context.Listings.AnyAsync(l => l.Id == listingId);
        if (!listingExists)
        {
            throw new KeyNotFoundException($"Listing with ID {listingId} not found.");
        }

        var duplicateCheck = await _context
            .ListingViews.Where(lv =>
                lv.ListingId == listingId && lv.ViewedAt > DateTime.UtcNow.AddMinutes(-30)
            )
            .Where(lv =>
                (userId.HasValue && lv.UserId == userId)
                || (!string.IsNullOrEmpty(deviceUuid) && lv.DeviceUuid == deviceUuid)
            )
            .FirstOrDefaultAsync();

        if (duplicateCheck != null)
        {
            return duplicateCheck;
        }

        var view = new ListingView
        {
            Id = Guid.NewGuid(),
            ListingId = listingId,
            UserId = userId,
            DeviceUuid = deviceUuid,
            ViewedAt = DateTime.UtcNow,
            IpAddress = ipAddress,
            UserAgent = userAgent,
        };

        _context.ListingViews.Add(view);
        await _context.SaveChangesAsync();

        return view;
    }

    public async Task<int> GetViewCountAsync(Guid listingId)
    {
        return await _context.ListingViews.Where(lv => lv.ListingId == listingId).CountAsync();
    }

    public async Task<int> GetUniqueViewCountAsync(Guid listingId)
    {
        var uniqueUserViews = await _context
            .ListingViews.Where(lv => lv.ListingId == listingId && lv.UserId.HasValue)
            .Select(lv => lv.UserId)
            .Distinct()
            .CountAsync();

        var uniqueDeviceViews = await _context
            .ListingViews.Where(lv =>
                lv.ListingId == listingId
                && !lv.UserId.HasValue
                && !string.IsNullOrEmpty(lv.DeviceUuid)
            )
            .Select(lv => lv.DeviceUuid)
            .Distinct()
            .CountAsync();

        return uniqueUserViews + uniqueDeviceViews;
    }

    public async Task<Dictionary<Guid, int>> GetViewCountsAsync(List<Guid> listingIds)
    {
        return await _context
            .ListingViews.Where(lv => listingIds.Contains(lv.ListingId))
            .GroupBy(lv => lv.ListingId)
            .ToDictionaryAsync(g => g.Key, g => g.Count());
    }

    public async Task<List<Guid>> GetPopularListingIds(int limit = 10, DateTime? since = null)
    {
        var sinceDate = since ?? DateTime.UtcNow.AddDays(-30);

        return await _context
            .ListingViews.Where(lv => lv.ViewedAt >= sinceDate)
            .GroupBy(lv => lv.ListingId)
            .OrderByDescending(g => g.Count())
            .Take(limit)
            .Select(g => g.Key)
            .ToListAsync();
    }
}
