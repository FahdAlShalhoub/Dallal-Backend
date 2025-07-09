using Dallal_Backend_v2.Controllers.Common.Dtos;
using Dallal_Backend_v2.Entities.Enums;

namespace Dallal_Backend_v2.Controllers.Common.Dtos;

public class ListingDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public ListingBrokerDto Broker { get; set; } = default!;
    public LocalizedStringDto Area { get; set; } = default!;
    public Guid AreaId { get; set; }
    public string Currency { get; set; } = default!;
    public decimal PricePerContract { get; set; }
    public int BedroomCount { get; set; }
    public int BathroomCount { get; set; }
    public decimal AreaInMetersSq { get; set; }
    public ListingType ListingType { get; set; }
    public PropertyType PropertyType { get; set; }
    public RentalContractPeriod? RentalContractPeriod { get; set; }
    public decimal PricePerYear { get; set; } = 0;
    public DateTime CreatedAt { get; set; }
    public CoordinateDto Location { get; set; }
    public bool IsFavorite { get; set; }
    public bool IsViewed { get; set; }
    public List<DocumentDto> Images { get; set; } = [];
    public List<DocumentDto> Videos { get; set; } = [];
}