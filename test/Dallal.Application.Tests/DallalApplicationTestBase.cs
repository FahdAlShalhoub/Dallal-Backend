using Volo.Abp.Modularity;

namespace Dallal;

public abstract class DallalApplicationTestBase<TStartupModule> : DallalTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
