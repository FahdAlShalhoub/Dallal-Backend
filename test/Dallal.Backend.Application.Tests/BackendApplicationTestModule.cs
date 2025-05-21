using Volo.Abp.Modularity;

namespace Dallal.Backend;

[DependsOn(
    typeof(BackendApplicationModule),
    typeof(BackendDomainTestModule)
)]
public class BackendApplicationTestModule : AbpModule
{

}
