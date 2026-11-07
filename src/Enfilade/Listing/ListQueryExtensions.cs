using System.Linq.Expressions;
using System.Reflection;

namespace Enfilade;

public static class ListQueryExtensions
{
    private static readonly MethodInfo Ascending = Sorter(nameof(Queryable.OrderBy));

    private static readonly MethodInfo Descending = Sorter(nameof(Queryable.OrderByDescending));

    private static readonly MethodInfo AscendingInMemory = Sorter(typeof(Enumerable), nameof(Enumerable.OrderBy));

    private static readonly MethodInfo DescendingInMemory = Sorter(typeof(Enumerable), nameof(Enumerable.OrderByDescending));

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

    internal static ListPage<TItem> ToPage<TItem>(this IEnumerable<TItem> source, ListQuery<TItem> query)
    {
        var conditions = query.Conditions.Select(condition => condition.Compile()).ToArray();
        IReadOnlyList<TItem> filtered = [.. conditions.Length == 0 ? source : source.Where(item => Matches(conditions, item))];
        var size = Math.Max(1, query.Size);
        return new ListPage<TItem>([.. OrderedInMemory(filtered, query).Skip((Math.Max(1, query.Page) - 1) * size).Take(size)], filtered.Count);
    }

    private static bool Matches<TItem>(Func<TItem, bool>[] conditions, TItem item)
    {
        foreach (var condition in conditions)
        {
            if (!condition(item))
                return false;
        }

        return true;
    }

    private static IQueryable<TItem> Ordered<TItem>(this IQueryable<TItem> source, ListQuery<TItem> query)
    {
        if (query.Order is null)
            return source;

        var (key, sorter) = SorterFor(query, Ascending, Descending);
        return (IQueryable<TItem>)sorter.Invoke(null, [source, key])!;
    }

    private static IEnumerable<TItem> OrderedInMemory<TItem>(IEnumerable<TItem> source, ListQuery<TItem> query)
    {
        if (query.Order is null)
            return source;

        var (key, sorter) = SorterFor(query, AscendingInMemory, DescendingInMemory);
        return (IEnumerable<TItem>)sorter.Invoke(null, [source, key.Compile()])!;
    }

    private static (LambdaExpression Key, MethodInfo Sorter) SorterFor<TItem>(ListQuery<TItem> query, MethodInfo ascending, MethodInfo descending)
    {
        var key = MemberKey.Unboxed(query.Order!.Body);
        var sorter = (query.Sort?.Descending == true ? descending : ascending).MakeGenericMethod(typeof(TItem), key.Type);
        return (Expression.Lambda(key, query.Order.Parameters), sorter);
    }

    private static MethodInfo Sorter(string name) => Sorter(typeof(Queryable), name);

    private static MethodInfo Sorter(Type owner, string name) =>
        owner.GetMethods().Single(method => method.Name == name && method.GetParameters().Length == 2);
}
