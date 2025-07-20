using Dallal_Backend_v2.Entities;
using Dallal_Backend_v2.Entities.Enums;
using Dallal_Backend_v2.Entities.Submissions;

namespace Dallal_Backend_v2.Repositories.Listings;

public interface IListingRepository : IRepository<Listing>
{
    Task<IEnumerable<Listing>> GetRecentListingsAsync(int count);
    Task<IEnumerable<Listing>> GetByBrokerIdAsync(Guid brokerId);
    Task<IEnumerable<Listing>> GetByStatusAsync(ListingStatus status);
    Task<IEnumerable<Listing>> GetByAreaIdAsync(Guid areaId);
    Task<IEnumerable<Listing>> GetByTypeAsync(ListingType type);
    Task<Listing?> GetByIdAsync(Guid id);
    Task<IEnumerable<Listing>> SearchAsync(
        string? query,
        Guid? areaId,
        ListingType? type,
        PropertyType? propertyType,
        decimal? minPrice,
        decimal? maxPrice
    );
    Task<int> GetCountByBrokerIdAsync(Guid brokerId);
    Task<int> GetCountByStatusAsync(ListingStatus status);
    Task UpdateStatusAsync(Guid id, ListingStatus status);
    Task<Listing?> GetListingWithDetailsAsync(Guid id);
    Task<IEnumerable<Listing>> GetBrokerListingsAsync(
        Guid brokerId,
        ListingStatus status,
        int page,
        int pageSize
    );
    Task<IEnumerable<Listing>> GetBrokerListingsFromSubmissionsAsync(
        Guid brokerId,
        SubmissionStatus status,
        int page,
        int pageSize
    );
    Task ArchiveListingAsync(Guid id);
    Task UnarchiveListingAsync(Guid id);
    Task<bool> ValidateListingOwnershipAsync(Guid listingId, Guid brokerId);
    Task<IEnumerable<Listing>> GetRecentListingsAsync(int days, int limit);
    Task<IEnumerable<Listing>> GetListingDetailAsync(Guid id, Guid? userId);
    Task<IEnumerable<Listing>> GetListingsByAreaAsync(List<Guid> areaIds);
    Task<IEnumerable<Listing>> GetListingsByCoordinatesAsync(
        double lat,
        double lng,
        double deltaLat,
        double deltaLng
    );
    Task<ItemsAndTotalCount<Listing>> GetBrokerListingsPaginatedAsync(
        Guid brokerId,
        ListingStatus status,
        int page,
        int pageSize
    );
    Task<Dictionary<Guid, Listing>> GetListingsByIdsWithDetailsAsync(List<Guid> listingIds);
}
