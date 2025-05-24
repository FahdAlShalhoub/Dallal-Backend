using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Dallal.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class DallalDbContextFactory : IDesignTimeDbContextFactory<DallalDbContext>
{
    public DallalDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        DallalEfCoreEntityExtensionMappings.Configure();

        var builder = new DbContextOptionsBuilder<DallalDbContext>().UseNpgsql(
            configuration.GetConnectionString("Default")
        );

        return new DallalDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../Dallal.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false);

        return builder.Build();
    }
}
