using Volo.Abp.Modularity;

namespace Dallal;

/* Inherit from this class for your domain layer tests. */
public abstract class DallalDomainTestBase<TStartupModule> : DallalTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
