using System.Threading.Tasks;

namespace Dallal.Backend.Data;

public interface IBackendDbSchemaMigrator
{
    Task MigrateAsync();
}
