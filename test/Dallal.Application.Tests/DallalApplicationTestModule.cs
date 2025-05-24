using Volo.Abp.Modularity;

namespace Dallal;

[DependsOn(
    typeof(DallalApplicationModule),
    typeof(DallalDomainTestModule)
)]
public class DallalApplicationTestModule : AbpModule
{

}
