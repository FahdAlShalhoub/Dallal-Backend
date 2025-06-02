using Dallal_Backend_v2.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dallal_Backend_v2;

public class DatabaseContext : DbContext
{
    public DbSet<Broker> Brokers { get; set; }
    public DbSet<Buyer> Buyers { get; set; }

    public DatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options)
    {
    }
}