using System;
using System.Collections.Generic;
using Dallal.Localization.Dtos;
using Volo.Abp.Application.Dtos;

namespace Dallal.Areas.Dtos;

public class AreaDto : FullAuditedEntityDto<Guid>
{
    public Guid? ParentId { get; set; }
    public AreaDto? Parent { get; set; }
    public List<AreaDto> Children { get; set; } = [];
    public LocalizedStringDto Name { get; set; } = default!;
}
