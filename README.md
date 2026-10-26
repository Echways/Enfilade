# Enfilade

A Blazor Server shell for admin panels. Register routable components as sections. Enfilade provides the navigation, theme, page chrome, dialogs, and error and reconnection handling.

```csharp
builder.Services.AddEnfilade()
    .AddSection<Home>("Home")
    .AddSection<Orders>("Orders");

app.MapEnfilade();
```

## Installation

```shell
dotnet add package Enfilade
```

Requires .NET 10.

## Quick start

```csharp
using Enfilade;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEnfilade(shell => shell.Brand = "Orders")
    .AddSection<Home>("Home")
    .AddSection<Orders>("Orders");

var app = builder.Build();
app.MapEnfilade();
app.Run();
```

Add the namespaces to `_Imports.razor`:

```razor
@using Enfilade
@using MudBlazor
```

`MapEnfilade` maps the panel, its static assets, and the language endpoint. It also adds request localization and antiforgery to the pipeline, so call it after any authentication middleware.

## Sections

A section is a routable Razor component. It appears in the navigation in the order of registration.

```razor
@page "/orders"
@inject OrderStore Store
@inject IDialogService Dialogs

<PageHead Count="_orders?.Count" UpdatedAt="_loadedAt" OnRefresh="LoadAsync">
    <Actions>
        <button type="button" class="btn-k pigment" @onclick="CreateAsync">New order</button>
    </Actions>
</PageHead>
<AutoRefresh OnTick="LoadAsync" />

@if (_orders is null)
{
    <Placeholder />
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

    private Task CreateAsync() => Dialogs.ShowSheetAsync<OrderForm>("New order");
}
```

- `PageHead` takes its title from the navigation and sets the document title.
- The section's assembly is added to the router, so nested routes such as `/orders/{id}` need no registration. Use `AddPages(assembly)` for an assembly without sections.
- Scoped CSS (`Orders.razor.css`) is linked automatically. Add global stylesheets with `AddStylesheet(path)`.
- Call `NavigationManager.NotFound()` to render the panel's 404 page.

| Method                                    | Description                                                                    |
| ----------------------------------------- | ------------------------------------------------------------------------------ |
| `AddSection<TPage>(string title)`       | Adds a navigation entry. The page needs an `@page` route without parameters. |
| `AddSection<TPage>(Func<string> title)` | Same, with a title evaluated on each render, for localized titles.             |
| `AddCaption(string text)`               | Adds a caption above the sections that follow it.                              |
| `AddPages(Assembly assembly)`           | Adds an assembly's routes without a navigation entry.                          |
| `AddStylesheet(string path)`            | Links a stylesheet from static assets.                                         |

## Actions

`Button` runs an action through `Act`: it asks for confirmation, blocks repeated clicks while the action runs, reports success and shows a readable error.

```razor
<Button Kind="ButtonKind.Danger" OnClick="() => Store.DeleteAsync(order)" Done="Order deleted"
        Confirm="@(new Confirm("Delete the order?", "The customer is notified.", "Delete", Danger: true))">
    Delete
</Button>
```

From code, inject `Act`:

```csharp
await Act.RunAsync(() => Store.DeleteAsync(order), done: "Order deleted");
```

- A `PanelException(message)` shows its message to the user as is. Any other exception shows a general message and is logged.
- After a successful action, everything on the page that reports to `CurrentPage` reloads. Pass `Refresh="false"` (`refresh: false`) to skip it.
- `RunAsync` returns whether the action ran to the end.

| Parameter         | Description                                                              |
| ----------------- | ------------------------------------------------------------------------ |
| `Kind`          | `Primary` (default), `Danger` or `Quiet`.                              |
| `OnClick`       | The action. The button is disabled and shows that it is busy until it ends. |
| `Confirm`       | `Confirm(Question, Message, Yes, Danger)` asked before the action.     |
| `Done`          | Message shown after success.                                             |
| `Refresh`       | Reloads the page's blocks after success. Default `true`.               |
| `Submit`        | Renders a submit button and leaves the work to the form.                 |
| `Busy`, `Disabled` | Show the button as busy or disabled from outside.                   |

Other attributes, such as `class` or `aria-label`, pass through to the `<button>`.

## Dialogs

```csharp
if (await Dialogs.ConfirmAsync("Delete the order?", "The customer is notified.", "Delete", danger: true))
    await Store.DeleteAsync(order);

var sheet = await Dialogs.ShowSheetAsync<OrderForm>("New order");
var result = await sheet.Result;
```

`ShowSheetAsync` opens a side sheet on desktop and a bottom sheet on phones. With the default `form: true`, a click outside the sheet does not close it. Pass `form: false` for read-only details.

A sheet component wraps its markup in `Sheet` and closes through MudBlazor's `IMudDialogInstance`:

```razor
<Sheet>
    <ChildContent>
        <div class="group">
            <MudTextField @bind-Value="_customer" Label="Customer" />
        </div>
    </ChildContent>
    <Actions>
        <button type="button" class="btn-k pigment" @onclick="Save">Create</button>
    </Actions>
</Sheet>

@code {
    [CascadingParameter]
    private IMudDialogInstance Dialog { get; set; } = default!;

    private string _customer = "";

    private void Save() => Dialog.Close(DialogResult.Ok(_customer));
}
```

## Components

| Component                    | Description                                                                                                           |
| ---------------------------- | --------------------------------------------------------------------------------------------------------------------- |
| `PageHead`                 | Page title with `Count`, `Summary`, `UpdatedAt` with `OnRefresh`, and `Actions`.                            |
| `Placeholder`              | Skeleton for content that is still loading.                                                                           |
| `AutoRefresh`              | Invokes `OnTick` every `AutoRefreshInterval` while the tab is visible.                                            |
| `Sheet`                    | Layout for a sheet: `ChildContent` and `Actions`.                                                                 |
| `Figure`                   | Large number with two caption lines (`First`, `Second`). `Alarm` marks it red. Group in `<div class="figs">`. |
| `Strips`                   | Horizontal bars for a ranking, from `Strip(Label, Value, Shown)`.                                                   |
| `Meter`                    | `Used / Limit` with a bar that turns ochre at 90% and red at 100%.                                                  |
| `Crumb`                    | Removable filter chip that links to the address without the filter.                                                   |
| `CopyLine`, `CopyButton` | A value with a copy button.                                                                                           |
| `KeyField`                 | A secret token in groups, with a copy button.                                                                         |
| `EntranceLayout`           | Full-screen layout without navigation, for sign-in pages: `@layout EntranceLayout`.                                 |
| `LanguageLink`             | Link to the next language, for pages outside the shell.                                                               |

MudBlazor inputs, selects, switches, checkboxes, tables, menus and snackbars are themed.

## Formatting

`Words` formats values consistently in the current culture.

| Method                         | Example                                                                    |
| ------------------------------ | -------------------------------------------------------------------------- |
| `Number(1284)`               | `1,284`                                                                  |
| `Size(1536)`                 | `1.5 KB`                                                                 |
| `Duration(1240)`             | `1.2 s`                                                                  |
| `Share(0.123)`               | `12.3%`                                                                  |
| `Day`, `Minute`, `Clock` | `2026-10-26`, `2026-10-26 14:05`, `14:05` (UTC)                      |
| `Middle(text, 10)`           | `tnk_AB…XYZ`                                                            |
| `Count(pattern, n)`          | ICU plurals: `{n, plural, one {{number} order} other {{number} orders}}` |

`Clipboard.CopyAsync(text)` copies from code and explains when the page is not served over HTTPS.

## Styling

| Class                                         | Use                                              |
| --------------------------------------------- | ------------------------------------------------ |
| `btn-k pigment`                             | Primary button.                                  |
| `btn-danger`                                | Destructive button.                              |
| `link`                                      | Quiet text button.                               |
| `tbl`, `tbl dense`                        | On `MudTable`. Add `num` to numeric columns. |
| `empty`                                     | Empty state: a heading, then an optional button. |
| `block`, `lead`                           | Titled section further down a page.              |
| `group`, `pair`, `hint`, `form-error` | Form layout inside a sheet.                      |
| `note`, `warn-t`, `bad-t`               | Secondary, warning and error text.               |
| `sr`                                        | Visible to screen readers only.                  |

Design tokens are CSS custom properties: `--night`, `--deep`, `--rule`, `--mist`, `--light`, `--klein`, `--glow`, `--vermilion`, `--ochre`, `--sans`, `--mono`.

## Authentication

Enfilade uses the host's authentication and does not provide its own.

- Without `LoginPath`, every page is public.
- With `LoginPath`, every page and endpoint requires an authenticated user, and anonymous visitors are redirected to `LoginPath`. Mark pages that stay public, including the sign-in page, with `[AllowAnonymous]`.
- `SignOutPath` adds a sign-out item under the user name.

## Localization

```csharp
builder.Services.AddEnfilade(shell =>
{
    shell.Language = "en";
    shell.Languages = ["en", "ru"];
});
```

With more than one language, the navigation shows a language switch. The choice is stored in the standard ASP.NET Core culture cookie. Built-in strings ship in English and Russian.

## Configuration

Options bind from the `Enfilade` configuration section. Code passed to `AddEnfilade` takes precedence. Invalid options fail at startup.

```json
{
  "Enfilade": {
    "Brand": "Orders",
    "LoginPath": "login",
    "Languages": ["en", "ru"]
  }
}
```

| Option                  | Default      | Description                                     |
| ----------------------- | ------------ | ----------------------------------------------- |
| `Brand`               | `Enfilade` | Name in the navigation and document titles.     |
| `Favicon`             | —           | Static asset path of the icon.                  |
| `Language`            | `en`       | Default language. Must be one of `Languages`. |
| `Languages`           | —           | Languages offered in the switch.                |
| `LoginPath`           | —           | Sign-in page. Makes the panel private.          |
| `SignOutPath`         | —           | Sign-out endpoint.                              |
| `AutoRefreshInterval` | `00:00:30` | Period of `AutoRefresh`.                      |

## Runtime behavior

- An exception in a page is contained: the navigation keeps working and the page offers a retry. The page recovers on navigation.
- When the connection drops, a reconnection dialog shows the next attempt. Paused circuits can be resumed.
- After navigation, focus moves to the page heading.
- Layouts honor safe-area insets, coarse pointers and `prefers-reduced-motion`.

## Section libraries

A section library exposes an extension on `EnfiladeBuilder`:

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

The host calls `services.AddEnfilade().AddOrders()`. The library's scoped CSS is linked automatically. Its `wwwroot` is served at `_content/{PackageId}/`.

## License

[GPL-3.0](https://github.com/Echways/Enfilade/blob/main/LICENSE)
