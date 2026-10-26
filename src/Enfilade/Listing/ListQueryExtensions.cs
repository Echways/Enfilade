using System.Linq.Expressions;
using System.Reflection;

namespace Enfilade;

public static class ListQueryExtensions
{
    private static readonly MethodInfo Ascending = Sorter(nameof(Queryable.OrderBy));

    private static readonly MethodInfo Descending = Sorter(nameof(Queryable.OrderByDescending));

    public static IQueryable<TItem> Filter<TItem>(this IQueryable<TItem> source, ListQuery<TItem> query) =>
        query.Conditions.Aggregate(source, (filtered, condition) => filtered.Where(condition));

    public static IQueryable<TItem> Slice<TItem>(this IQueryable<TItem> source, ListQuery<TItem> query)
    {
        var size = Math.Max(1, query.Size);
        return source.Ordered(query).Skip((Math.Max(1, query.Page) - 1) * size).Take(size);
    }

    public static ListPage<TItem> ToPage<TItem>(this IQueryable<TItem> source, ListQuery<TItem> query)
    {
        var filtered = source.Filter(query);
        return new ListPage<TItem>([.. filtered.Slice(query)], filtered.LongCount());
    }

    private static IQueryable<TItem> Ordered<TItem>(this IQueryable<TItem> source, ListQuery<TItem> query)
    {
        if (query.Order is not { } order)
            return source;

        var key = MemberKey.Unboxed(order.Body);
        var typed = Expression.Lambda(key, order.Parameters);
        var sorter = (query.Sort?.Descending == true ? Descending : Ascending).MakeGenericMethod(typeof(TItem), key.Type);
        return (IQueryable<TItem>)sorter.Invoke(null, [source, typed])!;
    }

    private static MethodInfo Sorter(string name) =>
        typeof(Queryable).GetMethods().Single(method => method.Name == name && method.GetParameters().Length == 2);
}
