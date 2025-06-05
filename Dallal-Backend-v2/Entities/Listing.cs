namespace Dallal_Backend_v2.Entities;

public class Listing
{
    public Guid Id { get; set; }
    public Broker Broker { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
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
    public decimal PricePerYear
    {
        get
        {
            if (ListingType == ListingType.Buy)
                return 0; // No annual price for purchases

            return RentalContractPeriod switch
            {
                Entities.RentalContractPeriod.Day => PricePerContract * 365,
                Entities.RentalContractPeriod.Month => PricePerContract * 12,
                Entities.RentalContractPeriod.Year => PricePerContract,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }
        private set { }
    }
}

public class ListingDetail
{
    public Guid Id { get; set; }
    public DetailsDefinition Definition { get; set; } = default!;
    public DetailsDefinitionOption Option { get; set; } = default!;
}

public class DetailsDefinition
{
    public Guid Id { get; set; }
    public LocalizedString Name { get; set; } = default!;
    public MultipleSearchBehavior Type { get; set; }
    public List<DetailsDefinitionOption> Options { get; set; } = [];
}

public class DetailsDefinitionOption
{
    public Guid Id { get; set; }
    public LocalizedString Name { get; set; } = default!;
}

public enum MultipleSearchBehavior
{
    And,
    Or,
}

public enum ListingType
{
    Rent,
    Buy
}

public enum PropertyType
{
    Land,
    Apartment,
    Villa,
    Floor,
    OfficeSpace,
}

public enum RentalContractPeriod
{
    Day,
    Month,
    Year,
}
