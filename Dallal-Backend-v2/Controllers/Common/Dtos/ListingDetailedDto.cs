using Dallal_Backend_v2.Controllers.Common.Dtos;

namespace Dallal_Backend_v2.Controllers.Common.Dtos;

public class ListingDetailedDto : ListingDto
{
    public List<ListingDetailDto> Details { get; set; } = default!;
}