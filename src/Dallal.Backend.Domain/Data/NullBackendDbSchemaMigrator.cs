using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Dallal.Backend.Data;

/* This is used if database provider does't define
 * IBackendDbSchemaMigrator implementation.
 */
public class NullBackendDbSchemaMigrator : IBackendDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
