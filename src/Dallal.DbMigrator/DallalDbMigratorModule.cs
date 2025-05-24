using Dallal.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace Dallal.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(DallalEntityFrameworkCoreModule),
    typeof(DallalApplicationContractsModule)
)]
public class DallalDbMigratorModule : AbpModule
{
}
