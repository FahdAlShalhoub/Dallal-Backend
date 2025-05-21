using Volo.Abp.Modularity;

namespace Dallal.Backend;

[DependsOn(
    typeof(BackendDomainModule),
    typeof(BackendTestBaseModule)
)]
public class BackendDomainTestModule : AbpModule
{

}
