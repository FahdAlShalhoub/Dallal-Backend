using System;
using System.Collections.Generic;
using Dallal.Areas;
using Dallal.Identity;
using Volo.Abp.Domain.Entities.Auditing;

namespace Dallal.Listings;

public class Listing : FullAuditedAggregateRoot<Guid>
{
    public Listing(Guid id)
        : base(id) { }

    public Listing() { }

    public Guid? BrokerId { get; set; }
    public BrokerIdentity? Broker { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;

    public Guid AreaId { get; set; } = default!;
    public Area Area { get; set; } = default!;
    public List<ListingDetail> Details { get; set; } = default!;
    public string Currency { get; set; } = default!;
    public decimal PricePerContract { get; set; }
    public decimal PricePerYear
    {
        get
        {
            if (ListingType == ListingType.Buy)
                return 0; // No annual price for purchases

            return RentalContractPeriod switch
            {
                Listings.RentalContractPeriod.Day => PricePerContract * 365,
                Listings.RentalContractPeriod.Month => PricePerContract * 12,
                Listings.RentalContractPeriod.Year => PricePerContract,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }
        private set { }
    }

    // Listing and property type
    public ListingType ListingType { get; set; }
    public PropertyType PropertyType { get; set; }
    public RentalContractPeriod? RentalContractPeriod { get; set; }

    // Property specifications
    public int BedroomCount { get; set; }
    public int BathroomCount { get; set; }
    public decimal AreaInMetersSq { get; set; }
}
