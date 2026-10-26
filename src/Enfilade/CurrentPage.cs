namespace Enfilade;

public sealed class CurrentPage
{
    private readonly List<PageSource> _sources = [];

    public long? Count => Shown?.Count;

    public DateTimeOffset? UpdatedAt => Shown?.At;

    public event Action? Changed;

    public event Func<Task>? Refreshed;

    private PageSource? Shown => _sources.FirstOrDefault(source => source.At is not null);

    public PageSource Track(Func<Task> refresh)
    {
        var source = new PageSource(this, refresh);
        _sources.Add(source);
        return source;
    }

    public async Task RefreshAsync()
    {
        foreach (var source in _sources.ToArray())
            await source.Refresh();
        foreach (var listener in Refreshed?.GetInvocationList().Cast<Func<Task>>() ?? [])
            await listener();
    }

    internal void Leave(PageSource source)
    {
        if (_sources.Remove(source))
            Changed?.Invoke();
    }

    internal void Announce() => Changed?.Invoke();
}

public sealed class PageSource : IDisposable
{
    private readonly CurrentPage _page;

    internal PageSource(CurrentPage page, Func<Task> refresh)
    {
        _page = page;
        Refresh = refresh;
    }

    internal Func<Task> Refresh { get; }

    internal long? Count { get; private set; }

    internal DateTimeOffset? At { get; private set; }

    public void Report(long count, DateTimeOffset at)
    {
        Count = count;
        At = at;
        _page.Announce();
    }

    public void Dispose() => _page.Leave(this);
}
