using System.Linq.Expressions;

namespace Dallal_Backend_v2.Helpers;

public static class QueryHelpers
{
    public static IQueryable<T> WhereIf<T>(
        this IQueryable<T> query,
        bool condition,
        Expression<Func<T, bool>> predicate
    )
    {
        return condition ? query.Where(predicate) : query;
    }
}
