using Dallal_Backend_v2.Entities.Enums;
using Dallal_Backend_v2.Entities.Users;

namespace Dallal_Backend_v2.Repositories;

public interface IBrokerRepository : IRepository<Broker>
{
    Task<IEnumerable<Broker>> GetByStatusAsync(BrokerStatus status);
    Task<IEnumerable<Broker>> GetPaginatedAsync(int page, int pageSize, BrokerStatus? status = null);
    Task<int> GetCountByStatusAsync(BrokerStatus status);
    Task<Broker?> GetByUserIdAsync(Guid userId);
    Task<Broker?> GetByIdAsync(Guid id);
}