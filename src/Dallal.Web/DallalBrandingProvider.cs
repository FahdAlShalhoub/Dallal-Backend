using Volo.Abp.Ui.Branding;
using Volo.Abp.DependencyInjection;
using Microsoft.Extensions.Localization;
using Dallal.Localization;

namespace Dallal.Web;

[Dependency(ReplaceServices = true)]
public class DallalBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<DallalResource> _localizer;

    public DallalBrandingProvider(IStringLocalizer<DallalResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
