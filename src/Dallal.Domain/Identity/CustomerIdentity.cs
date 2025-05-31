using System;
using Volo.Abp.Identity;

namespace Dallal.Identity;

public class CustomerIdentity : IdentityUser
{
    // Customer Data goes here
    public CustomerIdentity(Guid id, string email, Guid? tenantId = null)
        : base(id, id.ToString(), email, tenantId) { }
}
