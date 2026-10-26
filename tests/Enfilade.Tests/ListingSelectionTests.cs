using Enfilade.Tests.Hosts;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;

namespace Enfilade.Tests;

public class ListingSelectionTests : PanelContext
{
    [Fact]
    public async Task Selected_rows_reach_the_bar()
    {
        var listing = Render<SelectableListing>();
        listing.WaitForAssertion(() => Assert.Equal(25, listing.FindAll("tbody tr").Count));
        Assert.Empty(listing.FindAll(".chosen"));

        await ChooseAsync(listing, 1, true);
        await ChooseAsync(listing, 3, true);

        listing.WaitForAssertion(() => Assert.Contains(Words.Count(EnfiladeText.Chosen, 2), listing.Find(".chosen").TextContent));
        Assert.Equal("1,3", listing.Find(".picked").TextContent);
        Assert.Equal("polite", listing.Find(".chosen").ParentElement!.GetAttribute("aria-live"));
    }

    [Fact]
    public async Task Select_all_takes_the_current_page()
    {
        var listing = Render<SelectableListing>();
        listing.WaitForAssertion(() => Assert.Equal(25, listing.FindAll("tbody tr").Count));

        await ChooseAsync(listing, 0, true);

        listing.WaitForAssertion(() => Assert.Equal(25, listing.Find(".picked").TextContent.Split(',').Length));
    }

    [Fact]
    public async Task Selection_clears_when_the_page_changes()
    {
        var listing = Render<SelectableListing>();
        listing.WaitForAssertion(() => Assert.Equal(25, listing.FindAll("tbody tr").Count));
        await ChooseAsync(listing, 1, true);
        listing.WaitForElement(".chosen");

        Services.GetRequiredService<NavigationManager>().NavigateTo("?page=2");

        listing.WaitForAssertion(() => Assert.Contains("Клиент 26", listing.Find("tbody tr").TextContent));
        Assert.Empty(listing.FindAll(".chosen"));
    }

    [Fact]
    public void Live_listing_reloads_on_tick()
    {
        Services.Configure<EnfiladeOptions>(options => options.AutoRefreshInterval = TimeSpan.FromMilliseconds(50));
        JSInterop.Setup<bool>("enfilade.visible").SetResult(true);
        var calls = 0;

        Render<SelectableListing>(parameters => parameters
            .Add(host => host.Live, true)
            .Add(host => host.Load, (query, token) =>
            {
                calls++;
                return Order.PageAsync(query, token);
            }));

        WaitForAssertion(() => Assert.True(calls >= 2), TimeSpan.FromSeconds(5));
    }

    private static Task ChooseAsync(IRenderedComponent<SelectableListing> listing, int index, bool on)
    {
        var box = listing.FindComponents<MudCheckBox<bool>>()[index];
        return listing.InvokeAsync(() => box.Instance.ValueChanged.InvokeAsync(on));
    }

    private void WaitForAssertion(Action assertion, TimeSpan timeout)
    {
        var until = DateTime.UtcNow + timeout;
        while (true)
        {
            try
            {
                assertion();
                return;
            }
            catch (Xunit.Sdk.XunitException) when (DateTime.UtcNow < until)
            {
                Thread.Sleep(20);
            }
        }
    }
}
