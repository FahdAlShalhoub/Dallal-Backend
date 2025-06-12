namespace Dallal_Backend_v2.Helpers;

public static class ICollectionExtensions
{
    public static bool IsNullOrEmpty<T>(this ICollection<T>? collection) =>
        collection == null || collection.Count == 0;
}
