using Dallal_Backend_v2.Entities.Listings;
using Dallal_Backend_v2.Repositories.Listings;
using Dallal_Backend_v2.Repositories.ListingViews;

namespace Dallal_Backend_v2.Services;

public class ListingViewService(
    IListingViewRepository _listingViewRepository,
    IListingRepository _listingRepository
)
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
        var listingExists = await _listingRepository.AnyAsync(l => l.Id == listingId);
        if (!listingExists)
        {
            throw new KeyNotFoundException($"Listing with ID {listingId} not found.");
        }

        var duplicateCheck = await _listingViewRepository.GetExistingViewAsync(
            listingId,
            userId,
            deviceUuid
        );

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

        await _listingViewRepository.AddAsync(view);

        return view;
    }
}
