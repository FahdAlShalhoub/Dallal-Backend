using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Dallal.Listings.Dtos;

public class CreateUpdateListingDto
{
    public Guid? BrokerId { get; set; }

    [Required]
    [StringLength(maximumLength: 256)]
    public string Name { get; set; } = default!;

    [Required]
    [StringLength(maximumLength: 2000)]
    public string Description { get; set; } = default!;

    [Required]
    public Guid AreaId { get; set; }

    public List<CreateUpdateListingDetailDto> Details { get; set; } = [];

    // Price and financial properties
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Price per contract must be a positive value")]
    public decimal PricePerContract { get; set; }

    [Required]
    [StringLength(maximumLength: 10)]
    public string Currency { get; set; } = default!;

    // Listing and property type
    [Required]
    public ListingType ListingType { get; set; }

    [Required]
    public PropertyType PropertyType { get; set; }

    public RentalContractPeriod? RentalContractPeriod { get; set; }

    // Property specifications
    [Range(0, int.MaxValue, ErrorMessage = "Bedroom count must be a non-negative value")]
    public int BedroomCount { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Bathroom count must be a non-negative value")]
    public int BathroomCount { get; set; }

    [Required]
    [Range(0.1, double.MaxValue, ErrorMessage = "Area must be a positive value")]
    public decimal AreaInMetersSq { get; set; }
}
