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
    private ListingAddress? _address;

    private string? _addressUri;

    public ListingParts<CellDefinition<TItem>> Cells { get; } = new();

    public ListingParts<FilterDefinition<TItem>> Filters { get; } = new(Unique);

    public string? Prefix => prefix();

    public ListingAddress Address
    {
        get
        {
            var uri = navigation.Uri;
            var name = prefix();
            if (_address is null || !ReferenceEquals(uri, _addressUri) || _address.Prefix != name)
            {
                _address = ListingAddress.Read(uri, name);
                _addressUri = uri;
            }

            return _address;
        }
    }

    public bool Searches => searches();

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

    private static void Unique(IReadOnlyList<FilterDefinition<TItem>> filters, FilterDefinition<TItem> filter)
    {
        if (filters.Any(existing => existing.Name == filter.Name))
            throw new InvalidOperationException($"Enfilade: two filters of one listing are named \"{filter.Name}\"; give one of them another Name");
    }
}
