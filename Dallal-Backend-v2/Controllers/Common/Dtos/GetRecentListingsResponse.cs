namespace Dallal_Backend_v2.Controllers.Common.Dtos;

public class GetRecentListingsResponse
{
    public int RecentListingsCount { get; set; }
    public List<ListingDto> ListingsList { get; set; } = [];
}
