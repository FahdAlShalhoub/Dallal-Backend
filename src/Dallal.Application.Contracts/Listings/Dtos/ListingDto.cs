using System;
using System.Collections.Generic;
using Dallal.Areas.Dtos;
using Volo.Abp.Application.Dtos;

namespace Dallal.Listings.Dtos;

public class ListingDto : FullAuditedEntityDto<Guid>
{
    public Guid? BrokerId { get; set; }
    public string Image { get; set; } = "https://picsum.photos/500";
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public Guid AreaId { get; set; }
    public AreaDto Area { get; set; } = default!;
    public List<ListingDetailDto> Details { get; set; } = [];

    // Price and financial properties
    public decimal PricePerContract { get; set; }
    public string Currency { get; set; } = default!;
    public decimal PricePerYear { get; set; } // Auto-calculated property

    // Listing and property type
    public ListingType ListingType { get; set; }
    public PropertyType PropertyType { get; set; }
    public RentalContractPeriod? RentalContractPeriod { get; set; }

    // Property specifications
    public int BedroomCount { get; set; }
    public int BathroomCount { get; set; }
    public decimal AreaInMetersSq { get; set; }
}
