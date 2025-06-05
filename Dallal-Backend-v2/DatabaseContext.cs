using Dallal_Backend_v2.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dallal_Backend_v2;

public class DatabaseContext : DbContext
{
    public DbSet<Broker> Brokers { get; set; }
    public DbSet<Buyer> Buyers { get; set; }
    public DbSet<Listing> Listings { get; set; }
    public DbSet<Area> Areas { get; set; }

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
        Guid riyadhId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        Guid NorthRiyadhId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        Guid EastRiyadhId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        Guid AlYasmeenId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        Guid AlNaseemId = Guid.Parse("55555555-5555-5555-5555-555555555555");
        Guid AlRawdahId = Guid.Parse("66666666-6666-6666-6666-666666666666");

        CreateArea(riyadhId, "Riyadh", "الرياض", null, context);

        CreateArea(NorthRiyadhId, "North Riyadh", "شمال الرياض", riyadhId, context);
        CreateArea(EastRiyadhId, "East Riyadh", "شرق الرياض", riyadhId, context);

        CreateArea(AlYasmeenId, "Al Yasmeen", "الياسمين", NorthRiyadhId, context);

        CreateArea(AlNaseemId, "Al Naseem", "النسيم", EastRiyadhId, context);
        CreateArea(AlRawdahId, "Al Rawdah", "الروضة", EastRiyadhId, context);
    };
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
        context.SaveChanges();
    }
}
}