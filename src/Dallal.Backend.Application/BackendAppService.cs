using Dallal.Backend.Localization;
using Volo.Abp.Application.Services;

namespace Dallal.Backend;

/* Inherit your application services from this class.
 */
public abstract class BackendAppService : ApplicationService
{
    protected BackendAppService()
    {
        LocalizationResource = typeof(BackendResource);
    }
}
