using Dallal.Backend.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace Dallal.Backend.Permissions;

public class BackendPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(BackendPermissions.GroupName);

        //Define your own permissions here. Example:
        //myGroup.AddPermission(BackendPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<BackendResource>(name);
    }
}
