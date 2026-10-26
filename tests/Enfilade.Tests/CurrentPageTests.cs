namespace Enfilade.Tests;

public class CurrentPageTests
{
    private static readonly DateTimeOffset Moment = new(2026, 10, 26, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Page_shows_what_the_first_live_source_reported()
    {
        var page = new CurrentPage();
        using var first = page.Track(() => Task.CompletedTask);
        using var second = page.Track(() => Task.CompletedTask);
        first.Report(12, Moment);
        second.Report(3, Moment);

        Assert.Equal(12, page.Count);
        Assert.Equal(Moment, page.UpdatedAt);
    }

    [Fact]
    public void Disposed_source_leaves_the_page()
    {
        var page = new CurrentPage();
        var source = page.Track(() => Task.CompletedTask);
        source.Report(12, Moment);
        source.Dispose();

        Assert.Null(page.Count);
    }

    [Fact]
    public async Task Refresh_runs_every_source_then_tells_listeners()
    {
        var page = new CurrentPage();
        var calls = new List<string>();
        using var a = page.Track(() => { calls.Add("a"); return Task.CompletedTask; });
        using var b = page.Track(() => { calls.Add("b"); return Task.CompletedTask; });
        page.Refreshed += () => { calls.Add("listener"); return Task.CompletedTask; };

        await page.RefreshAsync();

        Assert.Equal(["a", "b", "listener"], calls);
    }
}
