using Dallal.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace Dallal.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class DallalController : AbpControllerBase
{
    protected DallalController()
    {
        LocalizationResource = typeof(DallalResource);
    }
}
