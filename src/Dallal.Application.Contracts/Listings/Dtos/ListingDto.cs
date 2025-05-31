using System;
using System.Collections.Generic;
using Dallal.Areas.Dtos;
using Volo.Abp.Application.Dtos;

namespace Dallal.Listings.Dtos;

public class ListingDto : FullAuditedEntityDto<Guid>
{
    public Guid? BrokerId { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public Guid AreaId { get; set; }
    public AreaDto Area { get; set; } = default!;
    public List<ListingDetailDto> Details { get; set; } = [];
}
