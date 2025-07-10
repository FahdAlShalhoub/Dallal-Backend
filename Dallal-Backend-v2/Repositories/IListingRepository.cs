using Dallal_Backend_v2.Entities;
using Dallal_Backend_v2.Entities.Enums;

namespace Dallal_Backend_v2.Repositories;

public interface IListingRepository : IRepository<Listing>
{
    Task<IEnumerable<Listing>> GetRecentListingsAsync(int count);
    Task<IEnumerable<Listing>> GetByBrokerIdAsync(Guid brokerId);
    Task<IEnumerable<Listing>> GetByStatusAsync(ListingStatus status);
    Task<IEnumerable<Listing>> GetByAreaIdAsync(Guid areaId);
    Task<IEnumerable<Listing>> GetByTypeAsync(ListingType type);
    Task<Listing?> GetDetailedByIdAsync(Guid id);
    Task<Listing?> GetByIdAsync(Guid id);
    Task<IEnumerable<Listing>> SearchAsync(string? query, Guid? areaId, ListingType? type, PropertyType? propertyType, decimal? minPrice, decimal? maxPrice);
    Task<int> GetCountByBrokerIdAsync(Guid brokerId);
    Task<int> GetCountByStatusAsync(ListingStatus status);
    Task UpdateStatusAsync(Guid id, ListingStatus status);
}