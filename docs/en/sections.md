# Sections

[English](sections.md) · [Русский](../ru/sections.md) · [Documentation](README.md)

A section is a routable Razor component. It appears in the navigation in the order of registration.

```razor
@page "/orders"
@inject OrderStore Store
@inject IDialogService Dialogs

<PageHead>
    <Actions>
        <Button OnClick="CreateAsync" Refresh="false">New order</Button>
    </Actions>
</PageHead>
<Listing TItem="Order" Load="Store.PageAsync" SearchBy="[order => order.Customer]" Sort="-createdAt"
         Href="@(order => $"orders/{order.Id}")" Live>
    <Filters>
        <Search />
        <Choice Title="Status" By="order => order.Status" Options="Statuses" />
    </Filters>
    <Columns>
        <Cell Title="Customer" Value="order => order.Customer" Sort />
        <Cell Title="Total" Value="order => order.Total" Sort />
        <Cell Title="Created" Value="order => order.CreatedAt" Sort />
    </Columns>
</Listing>

@code {
    private static readonly IReadOnlyList<Option<OrderStatus>> Statuses =
        [new(OrderStatus.New, "New"), new(OrderStatus.Paid, "Paid")];

    private Task CreateAsync() => Dialogs.ShowFormAsync<OrderSheet, OrderDraft>("New order");
}
```

`OrderSheet` is a `Sheet` with a `Form` of `Field`s; see [Dialogs and forms](forms.md).

- `PageHead` takes its title from the navigation and sets the document title.
- The section's assembly is added to the router, so nested routes such as `/orders/{id}` need no registration. Use `AddPages(assembly)` for an assembly without sections.
- Scoped CSS (`Orders.razor.css`) is linked automatically. Add global stylesheets with `AddStylesheet(path)`.
- Call `NavigationManager.NotFound()` to render the panel's 404 page.

## Icons and badges

```csharp
.AddSection<Orders>("Orders", section => section
    .Icon(Icons.Material.Outlined.ShoppingCart)
    .Badge<OrderStore>(store => store.CountPendingAsync()))
```

- `Icon` takes an SVG path, such as the MudBlazor icons.
- A badge counts on navigation, after an action through `Button`, `Act` or `Form` (any `CurrentPage.RefreshAsync()`), and every `AutoRefreshInterval` while the tab is visible. Zero shows nothing.
- A failing count is logged and leaves the navigation and the other badges alone.
- Badges count one after another, so they can share a scoped `DbContext`.

## Builder methods

| Method                                    | Description                                                                    |
| ----------------------------------------- | ------------------------------------------------------------------------------ |
| `AddSection<TPage>(string title)`       | Adds a navigation entry. The page needs an `@page` route without parameters. |
| `AddSection<TPage>(Func<string> title)` | Same, with a title evaluated on each render, for localized titles.             |
| `AddSection<TPage>(title, section => ...)` | Same, with `section.Icon(svg)` and `section.Badge<TService>(service => service.CountAsync())`. |
| `AddAccountLink(title, href)`           | Adds a link such as "Profile" or "API keys" to the account menu; `title` can be a `Func<string>`. |
| `AddCaption(string text)`               | Adds a caption above the sections that follow it.                              |
| `AddPages(Assembly assembly)`           | Adds an assembly's routes without a navigation entry.                          |
| `AddStylesheet(string path)`            | Links a stylesheet from static assets.                                         |
| `ShowMessageOf<TException>(message)`    | Shows the message of an exception to the user; see [Error messages](errors.md). |

## Captions and account links

Captions split a long menu into groups. Rights follow the pages: a caption with no visible sections after it disappears, see [Authentication](authentication.md).

```csharp
builder.Services.AddEnfilade()
    .AddSection<Home>("Home")
    .AddCaption("Shop")
    .AddSection<Orders>("Orders")
    .AddSection<Customers>("Customers")
    .AddCaption("Account")
    .AddAccountLink("Profile", "profile");
```

## Sections in separate libraries

A section can live in its own package and register itself with one call. See [Section libraries](libraries.md).
