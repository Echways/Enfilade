using Enfilade.Tests.Hosts;
using Microsoft.Extensions.DependencyInjection;

namespace Enfilade.Tests;

public class PageHeadTests : PanelContext
{
    [Fact]
    public void Head_follows_the_listing()
    {
        var calls = 0;
        var host = Render<HeadedListing>(parameters => parameters.Add(head => head.Load, (query, token) =>
        {
            calls++;
            return Order.PageAsync(query, token);
        }));

        host.WaitForAssertion(() => Assert.Equal(Words.Number(60), host.Find("sup.count").TextContent));
        host.Find(".stamp").Click();

        host.WaitForAssertion(() => Assert.Equal(2, calls));
    }

    [Fact]
    public void Explicit_count_wins()
    {
        using var source = Services.GetRequiredService<CurrentPage>().Track(() => Task.CompletedTask);
        source.Report(60, DateTimeOffset.UtcNow);

        var head = Render<PageHead>(parameters => parameters.Add(head => head.Title, "Заказы").Add(head => head.Count, 5));

        Assert.Equal(Words.Number(5), head.Find("sup.count").TextContent);
        Assert.Empty(head.FindAll(".stamp"));
    }
}
