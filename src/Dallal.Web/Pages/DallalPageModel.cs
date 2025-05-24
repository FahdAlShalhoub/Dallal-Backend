using Dallal.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace Dallal.Web.Pages;

public abstract class DallalPageModel : AbpPageModel
{
    protected DallalPageModel()
    {
        LocalizationResourceType = typeof(DallalResource);
    }
}
