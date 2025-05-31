using System;
using System.Collections.Generic;
using Dallal.Localization;
using Volo.Abp.Domain.Entities.Auditing;

namespace Dallal.Areas;

public class Area : FullAuditedAggregateRoot<Guid>
{
    public Area(Guid id)
        : base(id) { }

    public Area() { }

    public Area? Parent { get; set; }
    public List<Area> Children { get; set; } = [];
    public LocalizedString Name { get; set; } = default!;
}
