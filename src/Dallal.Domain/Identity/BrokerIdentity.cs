using System;
using Volo.Abp.Identity;

namespace Dallal.Identity;

public class BrokerIdentity : IdentityUser
{
    // Broker Data goes here
    public BrokerIdentity(Guid id, string email, Guid? tenantId = null)
        : base(id, id.ToString(), email, tenantId) { }
}
