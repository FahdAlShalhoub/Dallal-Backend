using Dallal_Backend_v2.Entities.Enums;

namespace Dallal_Backend_v2.Controllers.Dtos;

public class GetListingsResponse
{
    public int RecentListingsCount { get; set; }
    public PaginatedList<ListingDto> ListingsList { get; set; }
}

public class ListingDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public BrokerDto Broker { get; set; } = default!;
    public LocalizedStringDto Area { get; set; } = default!;
    public string Currency { get; set; } = default!;
    public decimal PricePerContract { get; set; }
    public int BedroomCount { get; set; }
    public int BathroomCount { get; set; }
    public decimal AreaInMetersSq { get; set; }
    public ListingType ListingType { get; set; }
    public PropertyType PropertyType { get; set; }
    public RentalContractPeriod? RentalContractPeriod { get; set; }
    public List<ListingDetailDto> Details { get; set; } = default!;
    public decimal PricePerYear { get; set; } = 0;
    public DateTime CreatedAt { get; set; }
}

public record BrokerDto
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
}
