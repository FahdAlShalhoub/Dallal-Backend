using Dallal_Backend_v2.Entities.Enums;
using Dallal_Backend_v2.Entities.Users;

namespace Dallal_Backend_v2.Entities;

public class Listing
{
    public Guid Id { get; set; }
    public Guid BrokerId { get; set; }
    public Broker Broker { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public Guid AreaId { get; set; }
    public Area Area { get; set; } = default!;
    public string Currency { get; set; } = default!;
    public decimal PricePerContract { get; set; }
    public int BedroomCount { get; set; }
    public int BathroomCount { get; set; }
    public decimal AreaInMetersSq { get; set; }
    public ListingType ListingType { get; set; }
    public PropertyType PropertyType { get; set; }
    public RentalContractPeriod? RentalContractPeriod { get; set; }
    public List<ListingDetail> Details { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public decimal PricePerYear
    {
        get
        {
            if (ListingType == ListingType.Buy)
                return 0;

            return RentalContractPeriod switch
            {
                Enums.RentalContractPeriod.Day => PricePerContract * 365,
                Enums.RentalContractPeriod.Month => PricePerContract * 12,
                Enums.RentalContractPeriod.Year => PricePerContract,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }
        private set { }
    }

    public ListingStatus Status { get; set; }
}
