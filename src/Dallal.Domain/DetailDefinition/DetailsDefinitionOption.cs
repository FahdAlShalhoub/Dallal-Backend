using System;
using Dallal.Localization;
using Volo.Abp.Domain.Entities.Auditing;

namespace Dallal.DetailDefinition;

public class DetailsDefinitionOption : FullAuditedAggregateRoot<Guid>
{
    public DetailsDefinitionOption(Guid id)
        : base(id) { }

    public DetailsDefinitionOption() { }

    public LocalizedString Name { get; set; } = default!;
}
