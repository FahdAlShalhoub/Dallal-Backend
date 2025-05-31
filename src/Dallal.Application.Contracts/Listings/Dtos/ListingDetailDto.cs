using System;
using Volo.Abp.Application.Dtos;

namespace Dallal.Listings.Dtos;

public class ListingDetailDto : FullAuditedEntityDto<Guid>
{
    public Guid DefinitionId { get; set; }
    public Guid OptionId { get; set; }
}
