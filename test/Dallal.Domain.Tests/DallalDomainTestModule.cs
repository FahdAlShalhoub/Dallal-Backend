using Volo.Abp.Modularity;

namespace Dallal;

[DependsOn(
    typeof(DallalDomainModule),
    typeof(DallalTestBaseModule)
)]
public class DallalDomainTestModule : AbpModule
{

}
