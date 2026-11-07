using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;

namespace Enfilade.Tests;

public class AutoRefreshTests : PanelContext
{
    public AutoRefreshTests()
    {
        Services.Configure<EnfiladeOptions>(options => options.AutoRefreshInterval = TimeSpan.FromMilliseconds(20));
        JSInterop.Setup<bool>("enfilade.visible").SetResult(true);
    }

    [Fact]
    public void Failed_tick_reaches_the_error_boundary()
    {
        var boundary = Render<ErrorBoundary>(parameters => parameters
            .Add<AutoRefresh>(boundary => boundary.ChildContent, refresh => refresh
                .Add(refresh => refresh.OnTick, (Func<Task>)(() => throw new InvalidOperationException("Счётчик недоступен"))))
            .Add(boundary => boundary.ErrorContent, (Exception exception) => $"<p class=\"failed\">{exception.Message}</p>"));

        boundary.WaitForAssertion(() => Assert.Equal("Счётчик недоступен", boundary.Find(".failed").TextContent), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Ticks_keep_coming()
    {
        var ticks = 0;
        var third = new TaskCompletionSource();

        Render<AutoRefresh>(parameters => parameters.Add(refresh => refresh.OnTick, () =>
        {
            if (++ticks == 3)
                third.SetResult();
        }));

        await third.Task.WaitAsync(TimeSpan.FromSeconds(5), Xunit.TestContext.Current.CancellationToken);
    }
}
