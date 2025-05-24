using Volo.Abp.Settings;

namespace Dallal.Settings;

public class DallalSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(DallalSettings.MySetting1));
    }
}
