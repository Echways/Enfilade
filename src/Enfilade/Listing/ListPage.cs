namespace Enfilade;

public sealed record ListPage<TItem>(IReadOnlyList<TItem> Items, long Total);
