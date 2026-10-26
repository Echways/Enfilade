# Enfilade

A Blazor Server shell for admin panels. You write pages; Enfilade gives them a navigation rail, a theme, page headers, sheets, confirmations and a handful of building blocks. One line of setup per tab.

## Start

```csharp
using Enfilade;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEnfilade(shell => shell.Brand = "orders")
    .AddSection<Home>("Home")
    .AddSection<Orders>("Orders");

var app = builder.Build();
app.UseAntiforgery();
app.MapEnfilade();
app.Run();
```

Add `@using Enfilade` to your `_Imports.razor`, and `@using MudBlazor` when you use its components.

## Write a section

A section is an ordinary routable Razor component.

```razor
@page "/orders"
@inject OrderStore Store

<PageHead Count="_orders?.Count" UpdatedAt="_loadedAt" OnRefresh="LoadAsync">
    <Actions>
        <button type="button" class="btn-k pigment" @onclick="AddAsync">New order</button>
    </Actions>
</PageHead>
<AutoRefresh OnTick="LoadAsync" />

@if (_orders is null)
{
    <Placeholder />
}
else if (_orders.Count == 0)
{
    <div class="empty"><h2>No orders yet</h2></div>
}
else
{
    <MudTable Items="_orders" Elevation="0" Class="tbl">
        <HeaderContent>
            <MudTh>Customer</MudTh>
            <MudTh Class="num">Total</MudTh>
        </HeaderContent>
        <RowTemplate>
            <MudTd>@context.Customer</MudTd>
            <MudTd Class="num">@Words.Number(context.Total)</MudTd>
        </RowTemplate>
    </MudTable>
}

@code {
    private List<Order>? _orders;
    private DateTimeOffset? _loadedAt;

    protected override Task OnInitializedAsync() => LoadAsync();

    private async Task LoadAsync()
    {
        _orders = await Store.ListAsync();
        _loadedAt = DateTimeOffset.UtcNow;
    }
}
```

Register it with `.AddSection<Orders>("Orders")` and it shows up in the navigation, in the order of registration.

What Enfilade does for you:

- **Title.** `PageHead` takes the section's title from the navigation, and sets the browser tab title too. Pass `Title` only to override it.
- **Routing.** The section's assembly joins the router, so its other pages (`/orders/{id}`) work without extra setup. Add an assembly that has no section with `AddPages(assembly)`.
- **Styles.** Put an `Orders.razor.css` next to the page and it is applied. Blazor's scoped CSS bundle is linked automatically. Global stylesheets go through `AddStylesheet("_content/MyLib/my.css")`.
- **Access.** See [Sign in](#sign-in): once the panel has a login, every page is private unless it says otherwise.

Titles can be localized: `AddSection<Orders>(() => Text.Orders)` reads the title each time the navigation renders. `AddCaption("Access")` puts a caption above the sections that follow it.

## Ship a section as a package

A section library exposes one extension method on `EnfiladeBuilder`. It registers its own services through `Services`:

```csharp
public static class OrdersSection
{
    public static EnfiladeBuilder AddOrders(this EnfiladeBuilder shell)
    {
        shell.Services.AddScoped<OrderStore>();
        return shell.AddSection<Orders>(() => OrdersText.Title);
    }
}
```

The host then writes `services.AddEnfilade().AddOrders()`. Scoped CSS from the library is picked up with no further setup. A library's `wwwroot` is served at `_content/{PackageId}/`.

## Building blocks

| Component | What it is |
| --- | --- |
| `PageHead` | Page title with an optional `Count`, a `Summary`, an `UpdatedAt` stamp that refreshes on click (`OnRefresh`) and `Actions` on the right. |
| `Placeholder` | Breathing skeleton for content that is still loading. |
| `AutoRefresh` | Calls `OnTick` every `AutoRefreshInterval` while the tab is visible. |
| `Sheet` | Side sheet for forms and details. Open with `Dialogs.ShowAsync<MySheet>(title, Sheet.Form)` or `Sheet.View`. The sheet's content goes in `ChildContent`, its buttons in `Actions`. |
| `ConfirmDialog` | `await ConfirmDialog.AskAsync(dialogs, question, message, yes, danger)` returns `true` when confirmed. |
| `Figure` | A large number with two caption lines; `Alarm` paints it red. Group them in `<div class="figs">`. |
| `Strips` | Horizontal bars for a ranking (`Strip(label, value, shown)`). |
| `Meter` | `used / limit` with a bar that turns ochre at 90% and red at 100%. |
| `Crumb` | A removable filter chip that links to the address without the filter. |
| `CopyLine`, `CopyButton`, `KeyField` | A value with a copy button; `KeyField` shows a secret token in groups. |
| `LanguageLink` | Link to the next language, for pages outside the shell such as a sign-in page. |
| `EntranceLayout` | Full-screen layout without navigation, for sign-in and similar pages: `@layout EntranceLayout`. |

`Words` formats values the same way everywhere: `Number`, `Size`, `Duration`, `Share`, `Day`, `Minute`, `Clock`, `Middle` (cuts long text in the middle), and `Count` (ICU plurals: `{n, plural, one {{number} order} other {{number} orders}}`). `Clipboard` copies text from code and warns when the page is not on HTTPS.

MudBlazor components are themed and ready to use: `MudTable`, `MudMenu`, `MudTextField`, `MudSelect`, `MudSwitch`, `MudCheckBox`, snackbars and dialogs.

## Styling vocabulary

Prefer these classes to new CSS:

| Class | Use |
| --- | --- |
| `btn-k pigment` | Primary button. |
| `btn-danger` | Destructive button. |
| `link` | Quiet text button or link. |
| `tbl`, `tbl dense` | On `MudTable`. Add `num` to number columns. |
| `empty` | Empty state. An `h2` or `p` inside, a button below. |
| `block`, `lead` | A titled block further down the page (`<section class="block"><h2>…</h2><p class="lead">…</p>`). |
| `group`, `pair`, `hint`, `form-error` | Form layout inside a sheet: a field group, two fields side by side, help text, a validation error. |
| `note`, `warn-t`, `bad-t` | Secondary text, warning text, error text. |
| `sr` | Visible to screen readers only. |

Colors and fonts are CSS variables: `--night`, `--deep`, `--rule`, `--mist`, `--light`, `--klein`, `--glow`, `--vermilion`, `--ochre`, `--sans`, `--mono`.

## Sign in

Enfilade does not authenticate anyone. It works with whatever authentication the host sets up.

- Without `LoginPath` the panel is open, which suits local tools.
- With `LoginPath` the panel is private. Every page requires a signed-in user, the endpoints require authorization, and a stranger is sent to `LoginPath`. Mark a page `[AllowAnonymous]` to keep it open, for example the sign-in page itself.
- `SignOutPath` adds a "Sign out" item under the user's name.

```csharp
builder.Services.AddEnfilade(shell =>
{
    shell.LoginPath = "login";
    shell.SignOutPath = "logout";
});
```

## Languages

```csharp
shell.Language = "en";
shell.Languages = ["en", "ru"];
```

Call `app.UseEnfiladeLanguage()` before routing. With more than one language, a switch appears at the bottom of the navigation; the choice is kept in a cookie. Enfilade's own words come in English and Russian.

## Options

| Option | Default | |
| --- | --- | --- |
| `Brand` | `Enfilade` | Name at the top of the navigation and in tab titles. |
| `Favicon` | none | Path to the icon among static assets. |
| `Language`, `Languages` | `en`, none | Default language and the languages offered. |
| `LoginPath`, `SignOutPath` | none | See [Sign in](#sign-in). |
| `AutoRefreshInterval` | 30 s | Period of `AutoRefresh`. |

## License

GPL-3.0.
