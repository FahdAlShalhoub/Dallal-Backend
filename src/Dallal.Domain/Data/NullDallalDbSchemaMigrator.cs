using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Dallal.Data;

/* This is used if database provider does't define
 * IDallalDbSchemaMigrator implementation.
 */
public class NullDallalDbSchemaMigrator : IDallalDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
