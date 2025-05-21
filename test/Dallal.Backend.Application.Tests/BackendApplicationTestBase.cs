using Volo.Abp.Modularity;

namespace Dallal.Backend;

public abstract class BackendApplicationTestBase<TStartupModule> : BackendTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
