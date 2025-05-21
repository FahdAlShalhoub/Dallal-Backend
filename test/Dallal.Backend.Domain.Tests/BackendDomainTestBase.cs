using Volo.Abp.Modularity;

namespace Dallal.Backend;

/* Inherit from this class for your domain layer tests. */
public abstract class BackendDomainTestBase<TStartupModule> : BackendTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
