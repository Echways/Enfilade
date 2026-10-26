using Enfilade.Layout;
using Enfilade.Tests.Hosts;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace Enfilade.Tests;

public class ListingTests : PanelContext
{
    private NavigationManager Navigation => Services.GetRequiredService<NavigationManager>();

    [Fact]
    public void First_render_shows_the_skeleton_then_the_rows()
    {
        var pending = new TaskCompletionSource<ListPage<Order>>();
        var listing = Render<OrderListing>(parameters => parameters.Add(host => host.Load, (_, _) => pending.Task));

        Assert.NotEmpty(listing.FindAll(".placeholder"));
        pending.SetResult(Order.Sixty.AsQueryable().ToPage(new ListQuery<Order>()));

        listing.WaitForAssertion(() => Assert.Equal(25, listing.FindAll("tbody tr").Count));
        Assert.Empty(listing.FindAll(".placeholder"));
    }

    [Fact]
    public void Cells_format_by_type()
    {
        var listing = Render<OrderListing>(parameters => parameters.Add(host => host.Items, [new Order(1, "Анна", 1234.5m, OrderStatus.Paid)]));

        var total = listing.WaitForElement("td.num");
        Assert.Equal(Words.Of(1234.5m), total.TextContent.Trim());
        Assert.Contains(listing.FindAll("td"), cell => cell.TextContent.Trim() == "Оплачен");
    }

    [Fact]
    public void Sorting_writes_the_address()
    {
        var listing = Render<OrderListing>(parameters => parameters.Add(host => host.Items, Order.Sixty));

        SortButton(listing, "Сумма").Click();
        Assert.EndsWith("?sort=total", Navigation.Uri);

        listing.WaitForAssertion(() => SortButton(listing, "Сумма").Click());
        Assert.EndsWith("?sort=-total", Navigation.Uri);
    }

    [Fact]
    public void Address_drives_the_query()
    {
        Navigation.NavigateTo("?page=2&sort=-total");
        ListQuery<Order>? seen = null;

        Render<OrderListing>(parameters => parameters.Add(host => host.Load, (query, token) =>
        {
            seen = query;
            return Order.PageAsync(query, token);
        }));

        Assert.NotNull(seen);
        Assert.Equal(2, seen.Page);
        Assert.Equal(new SortOrder("total", true), seen.Sort);
        Assert.NotNull(seen.Order);
    }

    [Fact]
    public void Unknown_sort_key_is_ignored()
    {
        Navigation.NavigateTo("?sort=nosuchkey");
        ListQuery<Order>? seen = null;

        Render<OrderListing>(parameters => parameters.Add(host => host.Load, (query, token) =>
        {
            seen = query;
            return Order.PageAsync(query, token);
        }));

        Assert.NotNull(seen);
        Assert.Null(seen.Sort);
        Assert.Null(seen.Order);
    }

    [Fact]
    public void Unknown_sort_key_falls_back_to_the_default_sort()
    {
        Navigation.NavigateTo("?sort=nosuchkey");

        var listing = Render<OrderListing>(parameters => parameters.Add(host => host.Items, Order.Sixty).Add(host => host.Sort, "-total"));

        listing.WaitForAssertion(() => Assert.Contains(Words.Of(600m), listing.Find("tbody tr").TextContent));
    }

    [Fact]
    public void Page_past_the_end_shows_the_last_page()
    {
        Navigation.NavigateTo("?page=99");

        var listing = Render<OrderListing>(parameters => parameters.Add(host => host.Items, Order.Sixty));

        listing.WaitForAssertion(() => Assert.Contains(
            string.Format(EnfiladeText.PageRange, Words.Number(51), Words.Number(60), Words.Number(60)),
            listing.Find(".pager").TextContent));
        Assert.Equal(10, listing.FindAll("tbody tr").Count);
    }

    [Fact]
    public void Default_sort_applies_without_an_address()
    {
        var listing = Render<OrderListing>(parameters => parameters.Add(host => host.Items, Order.Sixty).Add(host => host.Sort, "-total"));

        listing.WaitForAssertion(() => Assert.Contains(Words.Of(600m), listing.Find("tbody tr").TextContent));
    }

    [Fact]
    public void Sortable_computed_cell_needs_a_key()
    {
        var error = Assert.Throws<InvalidOperationException>(() => Render<ComputedListing>());

        Assert.Contains("Двойная сумма", error.Message);
    }

    [Fact]
    public void Load_and_items_are_exclusive()
    {
        Assert.Throws<InvalidOperationException>(() => Render<OrderListing>());
        Assert.Throws<InvalidOperationException>(() => Render<OrderListing>(parameters => parameters
            .Add(host => host.Items, Order.Sixty)
            .Add(host => host.Load, Order.PageAsync)));
    }

    [Fact]
    public void Failed_load_stays_inside_the_listing()
    {
        var calls = 0;
        var listing = Render<OrderListing>(parameters => parameters.Add(host => host.Load, (query, token) =>
        {
            calls++;
            return calls == 1 ? throw new InvalidOperationException("database is down") : Order.PageAsync(query, token);
        }));

        listing.WaitForAssertion(() => Assert.Contains(EnfiladeText.LoadFailed, listing.Markup));
        listing.Find(".listing-failed button").Click();

        Assert.Equal(2, calls);
        listing.WaitForAssertion(() => Assert.Equal(25, listing.FindAll("tbody tr").Count));
        Assert.DoesNotContain(EnfiladeText.LoadFailed, listing.Markup);
    }

    [Fact]
    public void Stale_response_is_dropped()
    {
        var first = new TaskCompletionSource<ListPage<Order>>();
        var firstToken = CancellationToken.None;
        var calls = 0;
        var listing = Render<OrderListing>(parameters => parameters.Add(host => host.Load, (query, token) =>
        {
            if (++calls > 1)
                return Order.PageAsync(query, token);
            firstToken = token;
            return first.Task;
        }));

        Navigation.NavigateTo("?page=2");
        listing.WaitForAssertion(() => Assert.Contains("Клиент 26", listing.Find("tbody tr").TextContent));
        first.SetResult(Order.Sixty.AsQueryable().ToPage(new ListQuery<Order>()));

        Assert.True(firstToken.IsCancellationRequested);
        listing.WaitForAssertion(() => Assert.Contains("Клиент 26", listing.Find("tbody tr").TextContent));
    }

    [Fact]
    public void Reload_keeps_the_rows_on_screen()
    {
        var later = new TaskCompletionSource<ListPage<Order>>();
        var calls = 0;
        var listing = Render<OrderListing>(parameters => parameters.Add(host => host.Load, (query, token) =>
            ++calls == 1 ? Order.PageAsync(query, token) : later.Task));
        listing.WaitForAssertion(() => Assert.Equal(25, listing.FindAll("tbody tr").Count));

        _ = listing.InvokeAsync(() => listing.Instance.Listing.RefreshAsync());

        Assert.Equal(2, calls);
        Assert.Equal(25, listing.FindAll("tbody tr").Count);
        Assert.Empty(listing.FindAll(".placeholder"));
        later.SetResult(Order.Sixty.AsQueryable().ToPage(new ListQuery<Order>()));
    }

    [Fact]
    public async Task Leaving_the_page_mid_load_is_quiet()
    {
        var pending = new TaskCompletionSource<ListPage<Order>>();
        var token = CancellationToken.None;
        Render<OrderListing>(parameters => parameters.Add(host => host.Load, (_, cancel) =>
        {
            token = cancel;
            return pending.Task;
        }));

        await DisposeComponentsAsync();
        pending.SetResult(Order.Sixty.AsQueryable().ToPage(new ListQuery<Order>()));

        Assert.True(token.IsCancellationRequested);
    }

    [Fact]
    public async Task Leaving_the_page_loads_nothing()
    {
        AddAuthorization().SetAuthorized("anna");
        var store = new OrderStore();
        Services.AddSingleton(store);
        new EnfiladeBuilder(Services).AddSection<OrdersPage>("Заказы").AddSection<EveryonePage>("Все");
        Navigation.NavigateTo("orders?page=2");
        var routes = Render<Routes>();
        routes.WaitForAssertion(() => Assert.Contains("Клиент 26", routes.Find("tbody tr").TextContent));
        var listing = routes.FindComponent<Listing<Order>>().Instance;

        Navigation.NavigateTo("everyone");
        routes.WaitForAssertion(() => Assert.Empty(routes.FindAll("tbody tr")));
        await routes.InvokeAsync(listing.RefreshAsync);

        Assert.Equal(1, store.Calls);
    }

    [Fact]
    public void Rows_link_when_href_is_given()
    {
        var listing = Render<OrderListing>(parameters => parameters
            .Add(host => host.Items, Order.Sixty)
            .Add(host => host.Href, order => $"orders/{order.Id}"));

        listing.WaitForAssertion(() => Assert.Equal(25, listing.FindAll("tbody tr").Count));
        Assert.All(listing.FindAll("tbody tr"), row => Assert.StartsWith("orders/", row.QuerySelector("td:first-child a")?.GetAttribute("href")));
        Assert.Equal("orders/1", listing.Find("tbody tr a").GetAttribute("href"));
    }

    [Fact]
    public void Row_opens_from_the_keyboard()
    {
        Order? opened = null;
        var listing = Render<OrderListing>(parameters => parameters
            .Add(host => host.Items, Order.Sixty)
            .Add(host => host.OnOpen, (Order order) => opened = order));

        listing.WaitForElement("tbody tr td:first-child button").Click();

        Assert.Equal(1, opened?.Id);
    }

    [Fact]
    public void Empty_listing_says_so()
    {
        var plain = Render<OrderListing>(parameters => parameters.Add(host => host.Items, []));
        plain.WaitForAssertion(() => Assert.Contains(EnfiladeText.NothingYet, plain.Markup));

        var custom = Render<OrderListing>(parameters => parameters
            .Add(host => host.Items, [])
            .Add(host => host.Empty, "<p class=\"own\">Заказов нет</p>"));
        custom.WaitForElement(".own");
        Assert.DoesNotContain(EnfiladeText.NothingYet, custom.Markup);
    }

    [Fact]
    public void Listing_reports_to_the_page()
    {
        var time = new FakeTimeProvider(new DateTimeOffset(2026, 10, 26, 12, 0, 0, TimeSpan.Zero));
        Services.AddSingleton<TimeProvider>(time);
        var page = Services.GetRequiredService<CurrentPage>();

        var listing = Render<OrderListing>(parameters => parameters.Add(host => host.Load, Order.PageAsync));

        listing.WaitForAssertion(() => Assert.Equal(60, page.Count));
        Assert.Equal(time.GetUtcNow(), page.UpdatedAt);
    }

    private static AngleSharp.Dom.IElement SortButton(IRenderedComponent<OrderListing> listing, string title) =>
        listing.FindAll("th button").Single(button => button.TextContent.Contains(title));
}
