using System.Globalization;
using Microsoft.AspNetCore.WebUtilities;

namespace Enfilade;

internal sealed record ListingAddress(string? Prefix, int Page, string? Search, SortOrder? Sort, IReadOnlyDictionary<string, string> Filters)
{
    private const string PageKey = "page";

    private const string SortKey = "sort";

    private const string SearchKey = "q";

    private static readonly string[] OwnKeys = [PageKey, SortKey, SearchKey];

    public static ListingAddress Read(string uri, string? prefix)
    {
        var query = QueryHelpers.ParseQuery(new Uri(uri).Query)
            .Where(pair => !string.IsNullOrEmpty(pair.Value[^1]))
            .ToDictionary(pair => pair.Key, pair => pair.Value[^1]!, StringComparer.Ordinal);
        var start = prefix is null ? "" : prefix + ".";
        var page = int.TryParse(query.GetValueOrDefault(KeyOf(prefix, PageKey)), NumberStyles.None, CultureInfo.InvariantCulture, out var number)
            && number > 0 ? number : 1;
        var filters = query
            .Where(pair => pair.Key.Length > start.Length && pair.Key.StartsWith(start, StringComparison.Ordinal))
            .Where(pair => !OwnKeys.Contains(pair.Key[start.Length..]))
            .ToDictionary(pair => pair.Key[start.Length..], pair => pair.Value, StringComparer.Ordinal);
        return new ListingAddress(
            prefix, page, query.GetValueOrDefault(KeyOf(prefix, SearchKey)), SortOrder.Parse(query.GetValueOrDefault(KeyOf(prefix, SortKey))), filters);
    }

    public string Key(string name) => KeyOf(Prefix, name);

    public IReadOnlyDictionary<string, object?> ToQuery()
    {
        var query = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            [Key(PageKey)] = Page > 1 ? Page.ToString(CultureInfo.InvariantCulture) : null,
            [Key(SortKey)] = Sort?.ToString(),
            [Key(SearchKey)] = string.IsNullOrEmpty(Search) ? null : Search,
        };
        foreach (var (name, value) in Filters)
            query[Key(name)] = value;
        return query;
    }

    private static string KeyOf(string? prefix, string name) => prefix is null ? name : $"{prefix}.{name}";
}
