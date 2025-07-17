using Dallal_Backend_v2.Entities.Enums;
using Dallal_Backend_v2.Entities.Submissions;
using Dallal_Backend_v2.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Dallal_Backend_v2.Repositories.Brokers;

public class BrokerRepository(DatabaseContext context)
    : Repository<Broker>(context),
        IBrokerRepository
{
    public async Task<Dictionary<Guid, Broker>> GetBrokersByIdsWithUserAsync(List<Guid> brokerIds)
    {
        return await _dbSet
            .Where(b => brokerIds.Contains(b.Id))
            .Include(b => b.User)
            .ToDictionaryAsync(b => b.Id);
    }
}
