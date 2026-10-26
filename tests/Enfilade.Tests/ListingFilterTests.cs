using Bunit.TestDoubles;
using Enfilade.Tests.Hosts;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using MudBlazor;

namespace Enfilade.Tests;

public class ListingFilterTests : PanelContext
{
    private static readonly TimeSpan Pause = TimeSpan.FromMilliseconds(300);

    private readonly FakeTimeProvider _time = new();

    public ListingFilterTests() => Services.AddSingleton<TimeProvider>(_time);

    private BunitNavigationManager Navigation => (BunitNavigationManager)Services.GetRequiredService<NavigationManager>();

    [Fact]
    public async Task Choice_filters_and_shows_a_crumb()
    {
        var listing = Render<FilteredListing>();
        listing.WaitForAssertion(() => Assert.Equal(25, listing.FindAll("tbody tr").Count));

        await listing.InvokeAsync(() => listing.FindComponent<MudSelect<string>>().Instance.ValueChanged.InvokeAsync("Paid"));

        Assert.EndsWith("?status=Paid", Navigation.Uri);
        listing.WaitForAssertion(() => Assert.Contains(
            string.Format(EnfiladeText.PageRange, Words.Number(1), Words.Number(25), Words.Number(30)), listing.Find(".pager").TextContent));
        var crumb = listing.Find(".crumb");
        Assert.Contains("Статус", crumb.TextContent);
        Assert.Contains("Оплачен", crumb.TextContent);
        Assert.DoesNotContain("status", crumb.QuerySelector("a")!.GetAttribute("href"));
    }

    [Fact]
    public void Garbage_choice_in_the_address_is_ignored()
    {
        Navigation.NavigateTo("?status=garbage");

        var listing = Render<FilteredListing>();

        listing.WaitForAssertion(() => Assert.Contains(Words.Number(60), listing.Find(".pager").TextContent));
        Assert.Empty(listing.FindAll(".crumb"));
    }

    [Fact]
    public async Task Flag_filters_by_its_predicate()
    {
        var listing = Render<FilteredListing>();
        listing.WaitForAssertion(() => Assert.Equal(25, listing.FindAll("tbody tr").Count));

        await listing.InvokeAsync(() => listing.FindComponent<MudSwitch<bool>>().Instance.ValueChanged.InvokeAsync(true));

        Assert.EndsWith("?big=1", Navigation.Uri);
        listing.WaitForAssertion(() => Assert.Equal(10, listing.FindAll("tbody tr").Count));
    }

    [Fact]
    public void Search_waits_for_typing_to_pause()
    {
        var listing = Render<FilteredListing>();
        listing.WaitForAssertion(() => Assert.Equal(25, listing.FindAll("tbody tr").Count));

        listing.Find(".search input").Input("КЛИЕНТ 0");
        _time.Advance(Pause - TimeSpan.FromMilliseconds(1));
        Assert.DoesNotContain("q=", Navigation.Uri);

        _time.Advance(TimeSpan.FromMilliseconds(1));
        listing.WaitForAssertion(() => Assert.Contains("q=", Navigation.Uri));
        Assert.Equal("КЛИЕНТ 0", ListingAddress.Read(Navigation.Uri, null).Search);
        Assert.True(Navigation.History.First().Options.ReplaceHistoryEntry);
        listing.WaitForAssertion(() => Assert.Equal(9, listing.FindAll("tbody tr").Count));
    }

    [Fact]
    public void Fast_typing_keeps_only_the_last_query()
    {
        var searches = new List<string?>();
        var listing = Render<FilteredListing>(parameters => parameters.Add(host => host.Load, (query, token) =>
        {
            searches.Add(query.Search);
            return Order.PageAsync(query, token);
        }));
        listing.WaitForAssertion(() => Assert.Single(searches));

        listing.Find(".search input").Input("К");
        _time.Advance(Pause / 2);
        listing.Find(".search input").Input("КЛ");
        _time.Advance(Pause);

        listing.WaitForAssertion(() => Assert.Equal([null, "КЛ"], searches));
    }

    [Fact]
    public void Nothing_found_offers_a_reset()
    {
        Navigation.NavigateTo("?page=2&q=клиент&big=1&sort=total");

        var listing = Render<FilteredListing>(parameters => parameters.Add(host => host.Threshold, 10_000m));

        listing.WaitForAssertion(() => Assert.Contains(EnfiladeText.NothingFound, listing.Markup));
        var reset = listing.FindAll("a").Single(link => link.TextContent.Contains(EnfiladeText.ResetFilters));
        Assert.EndsWith("?sort=total", reset.GetAttribute("href"));
    }

    [Fact]
    public async Task Filter_change_returns_to_the_first_page()
    {
        Navigation.NavigateTo("?page=2");
        var listing = Render<FilteredListing>();
        listing.WaitForAssertion(() => Assert.Contains("Клиент 26", listing.Find("tbody tr").TextContent));

        await listing.InvokeAsync(() => listing.FindComponent<MudSelect<string>>().Instance.ValueChanged.InvokeAsync("New"));

        Assert.EndsWith("?status=New", Navigation.Uri);
    }

    [Fact]
    public void Duplicate_filter_names_are_rejected()
    {
        var error = Assert.Throws<InvalidOperationException>(() => Render<DuplicateFilters>());

        Assert.Contains("status", error.Message);
    }

    [Fact]
    public void Search_in_memory_needs_fields_to_search() =>
        Assert.Throws<InvalidOperationException>(() => Render<UnsearchableListing>());
}
