using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dallal.Areas;
using Dallal.Identity;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace Dallal.Listings;

/* Creates initial listing data with sample property listings
 * for demonstration and testing purposes.
 */
public class ListingDataSeedContributor(
    IRepository<Listing, Guid> _listingRepository,
    BrokerIdentityDataSeedContributor _brokerSeedContributor,
    AreaDataSeedContributor _areaSeedContributor,
    UnitOfWorkManager _unitOfWorkManager
) : IDataSeedContributor, ITransientDependency
{
    [UnitOfWork]
    public virtual async Task SeedAsync(DataSeedContext context)
    {
        await _brokerSeedContributor.SeedAsync(context);
        await _areaSeedContributor.SeedAsync(context);

        await _unitOfWorkManager.Current!.SaveChangesAsync();

        await CreateListingsAsync();
    }

    private async Task CreateListingsAsync()
    {
        // Predefined GUIDs for consistency
        var listing1Id = Guid.Parse("1111aaaa-1111-1111-1111-111111111111");
        var listing2Id = Guid.Parse("2222bbbb-2222-2222-2222-222222222222");
        var listing3Id = Guid.Parse("3333cccc-3333-3333-3333-333333333333");
        var listing4Id = Guid.Parse("4444dddd-4444-4444-4444-444444444444");
        var listing5Id = Guid.Parse("5555eeee-5555-5555-5555-555555555555");

        // Create listings
        await CreateListingAsync(
            listing1Id,
            "Luxury Villa in Al Yasmeen",
            "A beautiful 5-bedroom villa with modern amenities, private garden, and swimming pool in the heart of Al Yasmeen district.",
            BrokerIdentityDataSeedContributor.BobBuilderId,
            AreaDataSeedContributor.AlYasmeenId,
            ListingType.Rent,
            PropertyType.Villa,
            RentalContractPeriod.Year,
            5,
            4,
            500.00m,
            "SAR",
            120000.00m
        );

        await CreateListingAsync(
            listing2Id,
            "Modern Apartment for Sale",
            "A stunning 3-bedroom apartment with city view, located in a prime location in Al Naseem. Perfect for young families.",
            BrokerIdentityDataSeedContributor.DoraExplorerId,
            AreaDataSeedContributor.AlNaseemId,
            ListingType.Buy,
            PropertyType.Apartment,
            null,
            3,
            2,
            180.50m,
            "SAR",
            450000.00m
        );

        await CreateListingAsync(
            listing3Id,
            "Spacious Family Home",
            "A comfortable 4-bedroom house with garden in Al Rawdah. Great for families looking for a peaceful neighborhood.",
            BrokerIdentityDataSeedContributor.AbdoId,
            AreaDataSeedContributor.AlRawdahId,
            ListingType.Rent,
            PropertyType.Villa,
            RentalContractPeriod.Month,
            4,
            3,
            380.75m,
            "SAR",
            8500.00m
        );

        await CreateListingAsync(
            listing4Id,
            "Commercial Office Space",
            "Professional office space in Al Yasmeen business district. Ideal for startups and small businesses. Includes parking.",
            BrokerIdentityDataSeedContributor.BobBuilderId,
            AreaDataSeedContributor.AlYasmeenId,
            ListingType.Rent,
            PropertyType.OfficeSpace,
            RentalContractPeriod.Year,
            0,
            2,
            150.00m,
            "SAR",
            45000.00m
        );

        await CreateListingAsync(
            listing5Id,
            "Investment Land Plot",
            "Prime residential land plot in Al Naseem. Perfect for development or investment purposes. Easy access to main roads.",
            BrokerIdentityDataSeedContributor.DoraExplorerId,
            AreaDataSeedContributor.AlNaseemId,
            ListingType.Buy,
            PropertyType.Land,
            null,
            0,
            0,
            1200.00m,
            "SAR",
            800000.00m
        );
    }

    private async Task<Listing> CreateListingAsync(
        Guid id,
        string name,
        string description,
        Guid brokerId,
        Guid areaId,
        ListingType listingType,
        PropertyType propertyType,
        RentalContractPeriod? rentalContractPeriod,
        int bedroomCount,
        int bathroomCount,
        decimal areaInMetersSq,
        string currency,
        decimal pricePerContract
    )
    {
        // Check if listing already exists
        var existingListing = await _listingRepository.FindAsync(id);
        if (existingListing != null)
        {
            return existingListing;
        }

        // Create new listing (dependencies are guaranteed to exist)
        var listing = new Listing(id)
        {
            Name = name,
            Description = description,
            BrokerId = brokerId,
            AreaId = areaId,
            Details = new List<ListingDetail>(),
            Currency = currency,
            PricePerContract = pricePerContract,
            ListingType = listingType,
            PropertyType = propertyType,
            RentalContractPeriod = rentalContractPeriod,
            BedroomCount = bedroomCount,
            BathroomCount = bathroomCount,
            AreaInMetersSq = areaInMetersSq,
        };

        return await _listingRepository.InsertAsync(listing, autoSave: true);
    }
}
