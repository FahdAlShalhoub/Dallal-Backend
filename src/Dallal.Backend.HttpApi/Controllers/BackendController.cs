using Dallal.Backend.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace Dallal.Backend.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class BackendController : AbpControllerBase
{
    protected BackendController()
    {
        LocalizationResource = typeof(BackendResource);
    }
}
