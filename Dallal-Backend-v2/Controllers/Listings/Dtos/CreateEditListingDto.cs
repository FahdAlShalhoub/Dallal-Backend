using Dallal_Backend_v2.Controllers.Dtos;
using Dallal_Backend_v2.Entities.Enums;

namespace Dallal_Backend_v2.Controllers.Listings.Dtos;

public class CreateEditListingDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public Guid AreaId { get; set; }
    public string Currency { get; set; }
    public decimal PricePerContract { get; set; }
    public int BedroomCount { get; set; }
    public int BathroomCount { get; set; }
    public decimal AreaInMetersSq { get; set; }
    public ListingType ListingType { get; set; }
    public PropertyType PropertyType { get; set; }
    public RentalContractPeriod? RentalContractPeriod { get; set; }
    public CoordinateDto Location { get; set; }
    public List<DetailsDto> Details { get; set; }
}
