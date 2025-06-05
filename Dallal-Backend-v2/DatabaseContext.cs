using Dallal_Backend_v2.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dallal_Backend_v2;

public class DatabaseContext : DbContext
{
    public DbSet<Broker> Brokers { get; set; }
    public DbSet<Buyer> Buyers { get; set; }
    public DbSet<Listing> Listings { get; set; }
    public DbSet<Area> Areas { get; set; }

    private static Guid riyadhId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static Guid NorthRiyadhId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static Guid EastRiyadhId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static Guid AlYasmeenId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static Guid AlNaseemId = Guid.Parse("55555555-5555-5555-5555-555555555555");
    private static Guid AlRawdahId = Guid.Parse("66666666-6666-6666-6666-666666666666");


    // Public constants for broker IDs to be referenced by other seed contributors
    private static readonly Guid BobBuilderId = Guid.Parse("b0b00000-0000-0000-0000-000000000001");
    private static readonly Guid DoraExplorerId = Guid.Parse("d0d00000-0000-0000-0000-000000000002");
    private static readonly Guid AbdoId = Guid.Parse("abcd0000-0000-0000-0000-000000000003");

    public DatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options)
    {
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.Properties<LocalizedString>(p => { p.HaveColumnType("jsonb"); });

        configurationBuilder.Properties<Enum>().HaveConversion<string>();
    }

    public static Action<DbContext, bool> Seed()
    {
        return (context, _) =>
        {
            CreateAreas(context);
            CreateListings(context);
            CreateBrokers(context);

            context.SaveChanges();
        };
    }

    private static void CreateAreas(DbContext context)
    {
        CreateArea(riyadhId, "Riyadh", "الرياض", null, context);

        CreateArea(NorthRiyadhId, "North Riyadh", "شمال الرياض", riyadhId, context);
        CreateArea(EastRiyadhId, "East Riyadh", "شرق الرياض", riyadhId, context);

        CreateArea(AlYasmeenId, "Al Yasmeen", "الياسمين", NorthRiyadhId, context);

        CreateArea(AlNaseemId, "Al Naseem", "النسيم", EastRiyadhId, context);
        CreateArea(AlRawdahId, "Al Rawdah", "الروضة", EastRiyadhId, context);
    }

    private static void CreateArea(
        Guid areaId,
        string nameEn,
        string nameAr,
        Guid? parentId,
        DbContext context)
    {
        var existingArea = context.Find<Area>(areaId);
        if (existingArea == null)
        {
            var localizedName = new LocalizedString();
            localizedName.SetValue("en", nameEn);
            localizedName.SetValue("ar", nameAr);

            Area? parent = null;
            if (parentId.HasValue)
            {
                parent = context.Find<Area>(parentId.Value);
            }

            var area = new Area
            {
                Id = areaId,
                Name = localizedName,
                Parent = parent,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Set<Area>().Add(area);
        }
    }

    private static void CreateListings(DbContext context)
    {
        var listing1Id = Guid.Parse("1111aaaa-1111-1111-1111-111111111111");
        var listing2Id = Guid.Parse("2222bbbb-2222-2222-2222-222222222222");
        var listing3Id = Guid.Parse("3333cccc-3333-3333-3333-333333333333");
        var listing4Id = Guid.Parse("4444dddd-4444-4444-4444-444444444444");
        var listing5Id = Guid.Parse("5555eeee-5555-5555-5555-555555555555");

        // Create listings
        CreateListing(
            listing1Id,
            "Luxury Villa in Al Yasmeen",
            "A beautiful 5-bedroom villa with modern amenities, private garden, and swimming pool in the heart of Al Yasmeen district.",
            BobBuilderId,
            AlYasmeenId,
            ListingType.Rent,
            PropertyType.Villa,
            RentalContractPeriod.Year,
            5,
            4,
            500.00m,
            "SAR",
            120000.00m,
            context
        );

        CreateListing(
            listing2Id,
            "Modern Apartment for Sale",
            "A stunning 3-bedroom apartment with city view, located in a prime location in Al Naseem. Perfect for young families.",
            DoraExplorerId,
            AlNaseemId,
            ListingType.Buy,
            PropertyType.Apartment,
            null,
            3,
            2,
            180.50m,
            "SAR",
            450000.00m,
            context
        );

        CreateListing(
            listing3Id,
            "Spacious Family Home",
            "A comfortable 4-bedroom house with garden in Al Rawdah. Great for families looking for a peaceful neighborhood.",
            AbdoId,
            AlRawdahId,
            ListingType.Rent,
            PropertyType.Villa,
            RentalContractPeriod.Month,
            4,
            3,
            380.75m,
            "SAR",
            8500.00m,
            context
        );

        CreateListing(
            listing4Id,
            "Commercial Office Space",
            "Professional office space in Al Yasmeen business district. Ideal for startups and small businesses. Includes parking.",
            BobBuilderId,
            AlYasmeenId,
            ListingType.Rent,
            PropertyType.OfficeSpace,
            RentalContractPeriod.Year,
            0,
            2,
            150.00m,
            "SAR",
            45000.00m,
            context
        );

        CreateListing(
            listing5Id,
            "Investment Land Plot",
            "Prime residential land plot in Al Naseem. Perfect for development or investment purposes. Easy access to main roads.",
            DoraExplorerId,
            AlNaseemId,
            ListingType.Buy,
            PropertyType.Land,
            null,
            0,
            0,
            1200.00m,
            "SAR",
            800000.00m,
            context
        );
    }

    private static void CreateListing(
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
        decimal pricePerContract,
        DbContext context
    )
    {
        // Check if listing already exists
        var existingListing = context.Find<Listing>(id);
        if (existingListing == null)
        {
            var listing = new Listing
            {
                Id = id,
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
            context.Set<Listing>().Add(listing);
        }
    }


    private static void CreateBrokers(DbContext context)
    {
        // Create the brokers
        CreateBroker(
            BobBuilderId,
            "bob.builder@dallal.com",
            "Bob Builder",
            context
        );
        CreateBroker(
            DoraExplorerId,
            "dora.explorer@dallal.com",
            "Dora Explorer",
            context
        );
        CreateBroker(AbdoId, "abdo@dallal.com", "Abdo Lost", context);
    }

    private static void CreateBroker(
        Guid id,
        string email,
        string name,
        DbContext context
    )
    {
        var existingBroker = context.Find<Broker>(id);
        if (existingBroker == null)
        {
            var broker = new Broker
            {
                Id = id,
                Email = email,
                Name = name,
                Password = BCrypt.Net.BCrypt.HashPassword("12345678"),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                DeletedAt = null
            };
            context.Set<Broker>().Add(broker);
        }
    }
}