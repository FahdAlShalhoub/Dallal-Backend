using Dallal.Localization;
using Volo.Abp.Application.Services;

namespace Dallal;

/* Inherit your application services from this class.
 */
public abstract class DallalAppService : ApplicationService
{
    protected DallalAppService()
    {
        LocalizationResource = typeof(DallalResource);
    }
}
