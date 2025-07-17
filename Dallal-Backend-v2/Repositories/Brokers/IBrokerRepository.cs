using Dallal_Backend_v2.Entities.Enums;
using Dallal_Backend_v2.Entities.Submissions;
using Dallal_Backend_v2.Entities.Users;

namespace Dallal_Backend_v2.Repositories.Brokers;

public interface IBrokerRepository : IRepository<Broker>
{
    Task<Dictionary<Guid, Broker>> GetBrokersByIdsWithUserAsync(List<Guid> brokerIds);
}
