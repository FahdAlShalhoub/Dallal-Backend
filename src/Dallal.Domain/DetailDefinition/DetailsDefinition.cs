using System;
using System.Collections.Generic;
using Dallal.DetailDefinition;
using Dallal.Localization;
using Volo.Abp.Domain.Entities.Auditing;

namespace Dallal.DetailDefinition;

public class DetailsDefinition : FullAuditedAggregateRoot<Guid>
{
    public DetailsDefinition(Guid id)
        : base(id) { }

    public DetailsDefinition() { }

    public LocalizedString Name { get; set; } = default!;
    public MultipleSearchBehavior Type { get; set; }
    public List<DetailsDefinitionOption> Options { get; set; } = [];
}
