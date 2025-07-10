using Dallal_Backend_v2.Entities.Listings;

namespace Dallal_Backend_v2.Repositories;

public interface IListingViewRepository : IRepository<ListingView>
{
    Task<IEnumerable<ListingView>> GetByListingIdAsync(Guid listingId);
    Task<IEnumerable<ListingView>> GetByUserIdAsync(Guid userId);
    Task<int> GetViewCountAsync(Guid listingId);
    Task<int> GetUniqueViewCountAsync(Guid listingId);
    Task<ListingView?> GetExistingViewAsync(Guid listingId, Guid? userId, string? deviceUuid);
    Task<Dictionary<Guid, int>> GetViewCountsAsync(List<Guid> listingIds);
    Task<List<Guid>> GetPopularListingIds(int limit = 10, DateTime? since = null);
}