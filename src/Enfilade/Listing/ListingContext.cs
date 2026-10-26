using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;

namespace Enfilade;

internal sealed record CellDefinition<TItem>(string Title, string? Key, LambdaExpression? Order, bool Number, RenderFragment<TItem> Render);

internal abstract record FilterDefinition<TItem>(string Name, string Title)
{
    public abstract Expression<Func<TItem, bool>>? ConditionFor(string raw);

    public abstract string? Shown(string raw);
}

internal sealed class ListingContext<TItem>(NavigationManager navigation, Func<string?> prefix, Func<bool> searches)
{
    private readonly List<CellDefinition<TItem>> _cells = [];

    private readonly List<FilterDefinition<TItem>> _filters = [];

    public IReadOnlyList<CellDefinition<TItem>> Cells => _cells;

    public IReadOnlyList<FilterDefinition<TItem>> Filters => _filters;

    public ListingAddress Address => ListingAddress.Read(navigation.Uri, prefix());

    public bool Searches => searches();

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
        navigation.NavigateTo(UriFor(change), replace: replace);
        return Task.CompletedTask;
    }

    public string UriFor(Func<ListingAddress, ListingAddress> change)
    {
        var current = Address;
        var next = change(current);
        var query = next.ToQuery().ToDictionary(StringComparer.Ordinal);
        foreach (var name in current.Filters.Keys.Where(name => !next.Filters.ContainsKey(name)))
            query[next.Key(name)] = null;
        return navigation.GetUriWithQueryParameters(query);
    }

    public Task FilterAsync(string name, string? raw) => GoAsync(address => address with
    {
        Page = 1,
        Filters = raw is null
            ? address.Filters.Where(pair => pair.Key != name).ToDictionary(StringComparer.Ordinal)
            : new Dictionary<string, string>(address.Filters, StringComparer.Ordinal) { [name] = raw },
    });

    public string? Raw(FilterDefinition<TItem> filter) =>
        Address.Filters.GetValueOrDefault(filter.Name) is { } raw && filter.ConditionFor(raw) is not null ? raw : null;
}
