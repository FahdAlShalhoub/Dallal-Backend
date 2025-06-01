using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Volo.Abp;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.BlobStoring.Database.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.PostgreSql;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

namespace Dallal.EntityFrameworkCore;

[DependsOn(
    typeof(DallalDomainModule),
    typeof(AbpPermissionManagementEntityFrameworkCoreModule),
    typeof(AbpSettingManagementEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCorePostgreSqlModule),
    typeof(AbpBackgroundJobsEntityFrameworkCoreModule),
    typeof(AbpAuditLoggingEntityFrameworkCoreModule),
    typeof(AbpFeatureManagementEntityFrameworkCoreModule),
    typeof(AbpIdentityEntityFrameworkCoreModule),
    typeof(AbpOpenIddictEntityFrameworkCoreModule),
    typeof(AbpTenantManagementEntityFrameworkCoreModule),
    typeof(BlobStoringDatabaseEntityFrameworkCoreModule)
)]
public class DallalEntityFrameworkCoreModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        DallalEfCoreEntityExtensionMappings.Configure();
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<DallalDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
        });

        var configuration = context.Services.GetConfiguration();

        Configure(
            (Action<AbpDbContextOptions>)(
                options =>
                {
                    string connectionString = configuration.GetConnectionString("Default")!;
                    NpgsqlDataSourceBuilder npgsqlDataSourceBuilder = new NpgsqlDataSourceBuilder(
                        connectionString
                    )
                        .EnableDynamicJson()
                        .EnableRecordsAsTuples()
                        .EnableUnmappedTypes();

                    NpgsqlDataSource dataSource = npgsqlDataSourceBuilder.Build();
                    options.Configure(ctx =>
                    {
                        DbContextOptionsBuilder dbContextOptionsBuilder = ctx
                            .DbContextOptions.UseNpgsql(dataSource)
                            .EnableSensitiveDataLogging()
                            .EnableDetailedErrors();

                        if (connectionString.Contains("Pooling=false;"))
                            dbContextOptionsBuilder.EnableServiceProviderCaching(false);
                    });
                }
            )
        );
    }

    public override async Task OnApplicationInitializationAsync(
        ApplicationInitializationContext context
    )
    {
        await base.OnApplicationInitializationAsync(context);
        var dbContext = context.ServiceProvider.GetRequiredService<DallalDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}
