using Dallal_Backend_v2.Controllers.Common.Dtos;

namespace Dallal_Backend_v2.Controllers.Listings.QueryDtos;

public class DetailedListingQueryDto : ListingQueryDto
{
    public List<ListingDetailDto> Details { get; set; } = default!;
}