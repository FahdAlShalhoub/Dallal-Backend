using System.Threading.Tasks;

namespace Dallal.Data;

public interface IDallalDbSchemaMigrator
{
    Task MigrateAsync();
}
