using System.Linq.Expressions;
using Dallal_Backend_v2.Controllers.Dtos;
using Dallal_Backend_v2.Entities;
using Dallal_Backend_v2.QueryDtos;
using Dallal_Backend_v2.ThirdParty;

public static class ListingMapper
{
    public static Expression<Func<Listing, ListingQueryDto>> SelectToQueryDto(Guid? userIdOrNull) =>
        listing => new ListingQueryDto
        {
            Id = listing.Id,
            Name = listing.Name,

            BrokerId = listing.BrokerId,

            BrokerName = listing.Broker.User.FirstName + " " + listing.Broker.User.LastName,
            BrokerImage = listing.Broker.User.ProfileImage,
            BrokerPhoneNumber = listing.Broker.User.Phone,
            BrokerEmail = listing.Broker.User.Email,

            AreaName = listing.Area.Name,
            PricePerYear = listing.PricePerYear,
            PricePerContract = listing.PricePerContract,

            ListingType = listing.ListingType,
            PropertyType = listing.PropertyType,
            RentalContractPeriod = listing.RentalContractPeriod,
            CreatedAt = listing.CreatedAt,
            UpdatedAt = listing.UpdatedAt,
            Location = listing.Location,
            Status = listing.Status,
            Images = listing.Images,
            Videos = listing.Videos,
            AreaId = listing.AreaId,
            BedroomCount = listing.BedroomCount,
            BathroomCount = listing.BathroomCount,
            AreaInMetersSq = listing.AreaInMetersSq,
            Currency = listing.Currency,
            Description = listing.Description,
            IsFavorite =
                userIdOrNull.HasValue && listing.Favorites.Any(f => f.Id == userIdOrNull.Value),
        };

    public static async Task<ListingDto> SelectToDto(ListingQueryDto listing, S3 s3)
    {
        var images = new List<DocumentDto>();
        var videos = new List<DocumentDto>();
        foreach (var image in listing.Images)
            images.Add((await s3.CreateDocumentDto(image))!);
        foreach (var video in listing.Videos)
            videos.Add((await s3.CreateDocumentDto(video))!);

        return new ListingDto
        {
            Id = listing.Id,
            Name = listing.Name,
            Description = listing.Description,
            Broker = new ListingBrokerDto
            {
                Id = listing.BrokerId,
                Email = listing.BrokerName,
                PhoneNumber = listing.BrokerPhoneNumber,
                Image =
                    listing.BrokerImage != null
                        ? await s3.CreateDocumentDto(listing.BrokerImage)
                        : null,
                Name = listing.BrokerName,
            },
            Area = new LocalizedStringDto(listing.AreaName),
            Currency = listing.Currency,
            PricePerContract = listing.PricePerContract,
            BedroomCount = listing.BedroomCount,
            BathroomCount = listing.BathroomCount,
            AreaInMetersSq = listing.AreaInMetersSq,
            ListingType = listing.ListingType,
            PropertyType = listing.PropertyType,
            RentalContractPeriod = listing.RentalContractPeriod,
            PricePerYear = listing.PricePerYear,
            CreatedAt = listing.CreatedAt,
            Location = new CoordinateDto
            {
                Longitude = listing.Location.Coordinate.Y,
                Latitude = listing.Location.Coordinate.X,
            },
            IsFavorite = listing.IsFavorite,
            Images = images,
            Videos = videos,
        };
    }

    public static async Task<ListingDetailedDto> SelectToDetailedDto(
        DetailedListingQueryDto listing,
        S3 s3
    )
    {
        var images = new List<DocumentDto>();
        var videos = new List<DocumentDto>();
        foreach (var image in listing.Images)
            images.Add((await s3.CreateDocumentDto(image))!);
        foreach (var video in listing.Videos)
            videos.Add((await s3.CreateDocumentDto(video))!);

        return new ListingDetailedDto
        {
            Id = listing.Id,
            Name = listing.Name,
            Description = listing.Description,
            Broker = new ListingBrokerDto
            {
                Id = listing.BrokerId,
                Email = listing.BrokerName,
                PhoneNumber = listing.BrokerPhoneNumber,
                Image =
                    listing.BrokerImage != null
                        ? await s3.CreateDocumentDto(listing.BrokerImage)
                        : null,
                Name = listing.BrokerName,
            },
            Area = new LocalizedStringDto(listing.AreaName),
            Currency = listing.Currency,
            PricePerContract = listing.PricePerContract,
            BedroomCount = listing.BedroomCount,
            BathroomCount = listing.BathroomCount,
            AreaInMetersSq = listing.AreaInMetersSq,
            ListingType = listing.ListingType,
            PropertyType = listing.PropertyType,
            RentalContractPeriod = listing.RentalContractPeriod,
            PricePerYear = listing.PricePerYear,
            CreatedAt = listing.CreatedAt,
            Location = new CoordinateDto
            {
                Longitude = listing.Location.Coordinate.Y,
                Latitude = listing.Location.Coordinate.X,
            },
            IsFavorite = listing.IsFavorite,
            Images = images,
            Videos = videos,
            Details = listing.Details,
        };
    }

    public static Expression<Func<Listing, DetailedListingQueryDto>> SelectToDetailQueryDto(
        Guid? userIdOrNull
    ) =>
        listing => new DetailedListingQueryDto
        {
            Id = listing.Id,
            Name = listing.Name,
            Description = listing.Description,

            BrokerId = listing.BrokerId,
            BrokerName = listing.Broker.User.FirstName + " " + listing.Broker.User.LastName,
            BrokerImage = listing.Broker.User.ProfileImage,
            BrokerPhoneNumber = listing.Broker.User.Phone,
            BrokerEmail = listing.Broker.User.Email,

            AreaName = listing.Area.Name,
            PricePerYear = listing.PricePerYear,
            PricePerContract = listing.PricePerContract,
            ListingType = listing.ListingType,
            PropertyType = listing.PropertyType,
            RentalContractPeriod = listing.RentalContractPeriod,
            CreatedAt = listing.CreatedAt,
            UpdatedAt = listing.UpdatedAt,
            Location = listing.Location,
            Status = listing.Status,
            Images = listing.Images,
            Videos = listing.Videos,
            AreaId = listing.AreaId,
            BedroomCount = listing.BedroomCount,
            BathroomCount = listing.BathroomCount,
            AreaInMetersSq = listing.AreaInMetersSq,
            Currency = listing.Currency,
            IsFavorite =
                userIdOrNull.HasValue && listing.Favorites.Any(f => f.Id == userIdOrNull.Value),
            Details = listing.Details.Select(detail => new ListingDetailDto(detail)).ToList(),
        };
}
