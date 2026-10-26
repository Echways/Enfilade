using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Enfilade;

public sealed class ListQuery<TItem>
{
    public int Page { get; init; } = 1;

    public int Size { get; init; } = 25;

    public string? Search { get; init; }

    public SortOrder? Sort { get; init; }

    public IReadOnlyDictionary<string, string> Filters { get; init; } = ReadOnlyDictionary<string, string>.Empty;

    public IReadOnlyList<Expression<Func<TItem, bool>>> Conditions { get; init; } = [];

    public LambdaExpression? Order { get; init; }
}
