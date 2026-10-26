using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;

namespace Enfilade;

internal sealed record CellDefinition<TItem>(string Title, string? Key, LambdaExpression? Order, bool Number, RenderFragment<TItem> Render);

internal abstract record FilterDefinition<TItem>(string Name, string Title)
{
    public abstract Expression<Func<TItem, bool>>? ConditionFor(string raw);

    public abstract string? Shown(string raw);
}

internal sealed class ListingContext<TItem>(NavigationManager navigation, Func<string?> prefix)
{
    private readonly List<CellDefinition<TItem>> _cells = [];

    private readonly List<FilterDefinition<TItem>> _filters = [];

    public IReadOnlyList<CellDefinition<TItem>> Cells => _cells;

    public IReadOnlyList<FilterDefinition<TItem>> Filters => _filters;

    public ListingAddress Address => ListingAddress.Read(navigation.Uri, prefix());

    public void Add(CellDefinition<TItem> cell) => _cells.Add(cell);

    public void Replace(CellDefinition<TItem> old, CellDefinition<TItem> cell) => _cells[_cells.IndexOf(old)] = cell;

    public void Remove(CellDefinition<TItem> cell) => _cells.Remove(cell);

    public void Add(FilterDefinition<TItem> filter)
    {
        if (_filters.Any(existing => existing.Name == filter.Name))
            throw new InvalidOperationException($"Enfilade: two filters of one listing are named \"{filter.Name}\"; give one of them another Name");
        _filters.Add(filter);
    }

    public void Replace(FilterDefinition<TItem> old, FilterDefinition<TItem> filter) => _filters[_filters.IndexOf(old)] = filter;

    public void Remove(FilterDefinition<TItem> filter) => _filters.Remove(filter);

    public Task GoAsync(Func<ListingAddress, ListingAddress> change, bool replace = false)
    {
        var current = Address;
        var next = change(current);
        var query = next.ToQuery().ToDictionary(StringComparer.Ordinal);
        foreach (var name in current.Filters.Keys.Where(name => !next.Filters.ContainsKey(name)))
            query[next.Key(name)] = null;
        navigation.NavigateTo(navigation.GetUriWithQueryParameters(query), replace: replace);
        return Task.CompletedTask;
    }
}
