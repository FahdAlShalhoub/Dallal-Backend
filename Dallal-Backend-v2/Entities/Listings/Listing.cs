using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Dallal_Backend_v2.Entities.Enums;
using Dallal_Backend_v2.Entities.Listings;
using Dallal_Backend_v2.Entities.Users;
using Dallal_Backend_v2.Helpers;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;

namespace Dallal_Backend_v2.Entities;

public class Listing : BaseEntity
{
    public Guid BrokerId { get; set; }

    [DoNotIncludeInSubmission]
    public Broker Broker { get; set; } = default!;
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
    public ListingType Type { get; set; }
    public PropertyType PropertyType { get; set; }
    public RentalContractPeriod? RentalContractPeriod { get; set; }
    public List<ListingDetail> Details { get; set; } = default!;

    [Column(TypeName = "geometry (point)")]
    [JsonConverter(typeof(GeoJsonConverterFactory))]
    public Point Location { get; set; } = default!;
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

    [DoNotIncludeInSubmission]
    public List<Buyer> Favorites { get; set; } = [];

    [DoNotIncludeInSubmission]
    public List<ListingView> Views { get; set; } = [];

    public List<Document> Images { get; set; } = [];
    public List<Document> Videos { get; set; } = [];
}
