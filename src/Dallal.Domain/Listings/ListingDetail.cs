using System;
using Dallal.DetailDefinition;
using Volo.Abp.Domain.Entities.Auditing;

namespace Dallal.Listings;

public class ListingDetail : FullAuditedAggregateRoot<Guid>
{
    public DetailsDefinition Definition { get; set; } = default!;
    public Guid DefinitionId { get; set; }
    public DetailsDefinitionOption Option { get; set; } = default!;
    public Guid OptionId { get; set; }
}
