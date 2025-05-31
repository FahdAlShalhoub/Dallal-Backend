namespace Dallal.Permissions;

public static class DallalPermissions
{
    public const string GroupName = "Dallal";

    public static class Areas
    {
        public const string Default = GroupName + ".Areas";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    //Add your own permission names. Example:
    //public const string MyPermission1 = GroupName + ".MyPermission1";
}
