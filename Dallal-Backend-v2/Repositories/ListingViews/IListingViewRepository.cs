using Dallal_Backend_v2.Entities.Listings;

namespace Dallal_Backend_v2.Repositories.ListingViews;

public interface IListingViewRepository : IRepository<ListingView>
{
    Task<ListingView?> GetExistingViewAsync(Guid listingId, Guid? userId, string? deviceUuid);
}
