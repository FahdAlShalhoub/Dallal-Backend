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

    public static class Listings
    {
        public const string Default = GroupName + ".Listings";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }
}
