using Dallal.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace Dallal.Permissions;

public class DallalPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(DallalPermissions.GroupName);

        var areasPermission = myGroup.AddPermission(
            DallalPermissions.Areas.Default,
            L("Permission:Areas")
        );
        areasPermission.AddChild(DallalPermissions.Areas.Create, L("Permission:Areas.Create"));
        areasPermission.AddChild(DallalPermissions.Areas.Update, L("Permission:Areas.Update"));
        areasPermission.AddChild(DallalPermissions.Areas.Delete, L("Permission:Areas.Delete"));

        var listingsPermission = myGroup.AddPermission(
            DallalPermissions.Listings.Default,
            L("Permission:Listings")
        );
        listingsPermission.AddChild(
            DallalPermissions.Listings.Create,
            L("Permission:Listings.Create")
        );
        listingsPermission.AddChild(
            DallalPermissions.Listings.Update,
            L("Permission:Listings.Update")
        );
        listingsPermission.AddChild(
            DallalPermissions.Listings.Delete,
            L("Permission:Listings.Delete")
        );

        //Define your own permissions here. Example:
        //myGroup.AddPermission(DallalPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<DallalResource>(name);
    }
}
