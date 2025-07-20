using Dallal_Backend_v2.Entities;
using Dallal_Backend_v2.Entities.Users;

namespace Dallal_Backend_v2.Repositories.Buyers;

public interface IBuyerRepository : IRepository<Buyer>
{
    Task<IEnumerable<Listing>> GetFavoriteListingsAsync(Guid buyerId, int pageNumber, int pageSize);
    Task AddFavoriteListingsAsync(Guid buyerId, List<Guid> listingIds);
    Task RemoveFavoriteListingsAsync(Guid buyerId, List<Guid> listingIds);
    Task<Buyer?> GetByUserIdAsync(Guid userId);
}