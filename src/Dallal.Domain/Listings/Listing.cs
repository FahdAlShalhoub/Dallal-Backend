using System;
using System.Collections.Generic;
using Dallal.Areas;
using Dallal.Identity;
using Volo.Abp.Domain.Entities.Auditing;

namespace Dallal.Listings;

public class Listing : FullAuditedAggregateRoot<Guid>
{
    public Guid? BrokerId { get; set; }
    public BrokerIdentity? Broker { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;

    public Guid AreaId { get; set; } = default!;
    public Area Area { get; set; } = default!;
    public List<ListingDetail> Details { get; set; } = default!;
}
