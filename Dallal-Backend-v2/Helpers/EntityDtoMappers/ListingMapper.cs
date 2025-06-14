using System.Linq.Expressions;
using Dallal_Backend_v2.Controllers.Dtos;
using Dallal_Backend_v2.Entities;

public static class ListingMapper
{
    public static Expression<Func<Listing, ListingDto>> SelectToDto(Guid? userIdOrNull) =>
        listing => new ListingDto
        {
            Id = listing.Id,
            Name = listing.Name,
            Description = listing.Description,
            Broker = new BrokerDto
            {
                Id = listing.Broker.Id,
                Email = listing.Broker.Email,
                Name = listing.Broker.Name,
            },
            Area = new LocalizedStringDto(listing.Area.Name),
            Currency = listing.Currency,
            PricePerContract = listing.PricePerContract,
            BedroomCount = listing.BedroomCount,
            BathroomCount = listing.BathroomCount,
            AreaInMetersSq = listing.AreaInMetersSq,
            ListingType = listing.ListingType,
            PropertyType = listing.PropertyType,
            RentalContractPeriod = listing.RentalContractPeriod,
            Details = listing.Details.Select(detail => new ListingDetailDto(detail)).ToList(),
            PricePerYear = listing.PricePerYear,
            CreatedAt = listing.CreatedAt,
            Location = new CoordinateDto
            {
                Longitude = listing.Location.Coordinate.Y,
                Latitude = listing.Location.Coordinate.X,
            },
            IsFavorite = userIdOrNull.HasValue
                ? listing.Favorites.Any(f => f.Id == userIdOrNull.Value)
                : false,
        };
}
