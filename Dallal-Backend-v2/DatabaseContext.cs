using Dallal_Backend_v2.Entities;
using Dallal_Backend_v2.Entities.Details;
using Dallal_Backend_v2.Entities.Submissions;
using Dallal_Backend_v2.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Dallal_Backend_v2;

public class DatabaseContext : DbContext
{
    public DbSet<BaseUser> Users { get; set; }
    public DbSet<Broker> Brokers { get; set; }
    public DbSet<Buyer> Buyers { get; set; }
    public DbSet<Admin> Admins { get; set; }
    public DbSet<Listing> Listings { get; set; }
    public DbSet<ListingDetail> ListingDetails { get; set; }
    public DbSet<DetailsDefinition> DetailsDefinitions { get; set; }
    public DbSet<DetailsDefinitionOption> DetailsDefinitionOptions { get; set; }
    public DbSet<Area> Areas { get; set; }
    public DbSet<Submission> Submissions { get; set; }

    public DatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options) { }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.Properties<LocalizedString>(p =>
        {
            p.HaveColumnType("jsonb");
        });

        configurationBuilder.Properties<Enum>().HaveConversion<string>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Listing>(listing =>
        {
            listing.HasIndex(e => e.CreatedAt).IsDescending();
            listing.Property(e => e.Location).HasColumnType("geometry (point)").IsRequired();
        });

        modelBuilder.Entity<Buyer>(buyer =>
        {
            buyer
                .HasMany(b => b.FavoriteListings)
                .WithMany(b => b.Favorites)
                .UsingEntity(i => i.ToTable("BuyerFavoriteListings"));
        });
    }

    public static Action<DbContext, bool> Seed() => DatabaseSeeder.Seed();
}
