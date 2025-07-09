using Dallal_Backend_v2.Controllers.Dtos;
using Dallal_Backend_v2.Entities;
using Dallal_Backend_v2.Entities.Enums;
using NetTopologySuite.Geometries;

namespace Dallal_Backend_v2.QueryDtos;

public class ListingQueryDto
{
    public Guid Id { get; set; }
    public Guid BrokerId { get; set; }
    public string BrokerName { get; set; } = default!;
    public string? BrokerPhoneNumber { get; set; } = default!;
    public string? BrokerEmail { get; set; } = default!;
    public Document? BrokerImage { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public Guid AreaId { get; set; }
    public LocalizedString AreaName { get; set; } = default!;
    public string Currency { get; set; } = default!;
    public decimal PricePerContract { get; set; }
    public int BedroomCount { get; set; }
    public int BathroomCount { get; set; }
    public decimal AreaInMetersSq { get; set; }
    public ListingType ListingType { get; set; }
    public PropertyType PropertyType { get; set; }
    public RentalContractPeriod? RentalContractPeriod { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Geometry Location { get; set; } = default!;
    public decimal PricePerYear { get; set; }

    public ListingStatus Status { get; set; }
    public List<Document> Images { get; set; } = [];
    public List<Document> Videos { get; set; } = [];
    public bool IsFavorite { get; set; }
    public bool IsViewed { get; set; }
}

public class DetailedListingQueryDto : ListingQueryDto
{
    public List<ListingDetailDto> Details { get; set; } = default!;
}
