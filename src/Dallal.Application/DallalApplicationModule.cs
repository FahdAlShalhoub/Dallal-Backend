using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Account;
using Volo.Abp.AutoMapper;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.TenantManagement;

namespace Dallal;

[DependsOn(
    typeof(DallalDomainModule),
    typeof(DallalApplicationContractsModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpAccountApplicationModule),
    typeof(AbpTenantManagementApplicationModule),
    typeof(AbpSettingManagementApplicationModule)
)]
public class DallalApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<DallalApplicationModule>();
        });

        // Configure AWS S3
        ConfigureAwsS3(context, configuration);
    }

    private static void ConfigureAwsS3(
        ServiceConfigurationContext context,
        IConfiguration configuration
    )
    {
        var accessKeyId = configuration["S3:AccessKeyId"];
        var secretAccessKey = configuration["S3:SecretAccessKey"];
        var region = configuration["S3:Region"] ?? "us-east-1";

        context.Services.AddSingleton<IAmazonS3>(serviceProvider =>
        {
            var regionEndpoint = new AmazonS3Config { ServiceURL = region, ForcePathStyle = true };

            if (
                !string.IsNullOrWhiteSpace(accessKeyId)
                && !string.IsNullOrWhiteSpace(secretAccessKey)
            )
            {
                var credentials = new BasicAWSCredentials(accessKeyId, secretAccessKey);
                return new AmazonS3Client(credentials, regionEndpoint);
            }
            else
            {
                return new AmazonS3Client(regionEndpoint);
            }
        });
    }
}
