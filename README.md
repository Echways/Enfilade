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

`OrderSheet` is a `Sheet` with a `Form` of `Field`s; see [Forms](#forms).

- `PageHead` takes its title from the navigation and sets the document title.
- The section's assembly is added to the router, so nested routes such as `/orders/{id}` need no registration. Use `AddPages(assembly)` for an assembly without sections.
- Scoped CSS (`Orders.razor.css`) is linked automatically. Add global stylesheets with `AddStylesheet(path)`.
- Call `NavigationManager.NotFound()` to render the panel's 404 page.

```csharp
.AddSection<Orders>("Orders", section => section
    .Icon(Icons.Material.Outlined.ShoppingCart)
    .Badge<OrderStore>(store => store.CountPendingAsync()))
```

- `Icon` takes an SVG path, such as the MudBlazor icons.
- A badge counts on navigation, after an action through `Button`, `Act` or `Form` (any `CurrentPage.RefreshAsync()`), and every `AutoRefreshInterval` while the tab is visible. Zero shows nothing. A failing count is logged and leaves the navigation and the other badges alone. Badges count one after another, so they can share a scoped `DbContext`.

| Method                                    | Description                                                                    |
| ----------------------------------------- | ------------------------------------------------------------------------------ |
| `AddSection<TPage>(string title)`       | Adds a navigation entry. The page needs an `@page` route without parameters. |
| `AddSection<TPage>(Func<string> title)` | Same, with a title evaluated on each render, for localized titles.             |
| `AddSection<TPage>(title, section => ...)` | Same, with `section.Icon(svg)` and `section.Badge<TService>(service => service.CountAsync())`. |
| `AddAccountLink(title, href)`           | Adds a link such as "Profile" or "API keys" to the account menu; `title` can be a `Func<string>`. |
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

`CurrentPage` is the scoped service behind this. A custom block joins it with `using var source = CurrentPage.Track(ReloadAsync)` and calls `source.Report(count, at)` after each load; `PageHead` shows what the first source reported. `RefreshAsync()` reloads every source and then raises `Refreshed`; `Changed` fires when a report arrives.

## Listings

`Listing` shows a table with sorting, paging, filters and the loading, empty and error states. Its state lives in the address, so a link to a filtered page can be sent to a colleague.

```razor
<PageHead />
<Listing TItem="Order" Load="Store.PageAsync" Sort="-createdAt" Href="@(order => $"orders/{order.Id}")">
    <Columns>
        <Cell Title="Customer" Value="order => order.Customer" Sort />
        <Cell Title="Total" Value="order => order.Total" Sort />
        <Cell Title="Created" Value="order => order.CreatedAt" Sort />
        <Slot Title="Status"><Tag Tone="ToneOf(context)">@context.Status</Tag></Slot>
    </Columns>
</Listing>
```

A server-side `Load` gets a `ListQuery<TItem>` with resolved expressions, so a store over `IQueryable` (EF Core included) stays one line:

```csharp
public Task<ListPage<Order>> PageAsync(ListQuery<Order> query, CancellationToken token) =>
    Task.FromResult(db.Orders.ToPage(query));
```

`Filter(query)` applies the conditions, `Slice(query)` the order and the page, `ToPage(query)` both plus the count. For other stores the query also carries the raw values: `Page`, `Size`, `Search`, `Sort` (`Key`, `Descending`) and `Filters` by name.

Pass `Items` instead of `Load` to let the listing sort, filter and page an in-memory collection.

| Parameter        | Description                                                                                  |
| ---------------- | -------------------------------------------------------------------------------------------- |
| `Load`         | `(ListQuery<TItem>, CancellationToken) => Task<ListPage<TItem>>`. A newer load cancels an older one. |
| `Items`        | An in-memory collection. Exactly one of `Load` and `Items` is required.                  |
| `Columns`      | `Cell` and `Slot` components.                                                            |
| `Href`         | A row link: middle click and "open in a new tab" work.                                     |
| `OnOpen`       | Called with the row's item on click or Enter, for example to open a sheet.                   |
| `Sort`         | Default sort key, `-` for descending: `"-total"`.                                       |
| `PageSize`     | Rows per page. Default `25`.                                                               |
| `Name`         | Prefix of the address keys, for two listings on one page: `orders.page`.                   |
| `Empty`        | Content shown when there is nothing yet.                                                     |
| `Dense`        | Tighter rows.                                                                                |

`Cell` shows a value formatted by `Words.Of`: numbers, money, dates in the viewer's time zone, durations, `bool` and enums with `[Display(Name)]`.

| Parameter        | Description                                                                                  |
| ---------------- | -------------------------------------------------------------------------------------------- |
| `Title`        | Column heading.                                                                              |
| `Value`        | `order => order.Total`. Its member path, in camel case, is the sort key in the address.   |
| `Sort`         | Makes the column sortable. A computed `Value` also needs `Key`.                          |
| `Number`       | Right-aligns the column. Default: on for numeric values.                                     |
| `Format`       | Own formatting of the value.                                                                 |
| `ChildContent` | A template with `context`; the value still sorts.                                          |

`Slot` is a template-only column, such as actions; it never sorts.

### Filters

```razor
<Listing TItem="Order" Load="Store.PageAsync" SearchBy="[order => order.Customer, order => order.Email]">
    <Filters>
        <Search />
        <Choice Title="Status" By="order => order.Status" Options="Statuses" />
        <Flag Title="Large" Name="large" Where="order => order.Total > 1000" />
    </Filters>
    <Columns>...</Columns>
</Listing>
```

Active filters appear as `Crumb`s that remove them. When filters leave nothing, the listing says so and links to the address without them. A filter change returns to the first page.

| Filter    | Parameters                                                                                                                       |
| --------- | -------------------------------------------------------------------------------------------------------------------------------- |
| `Search` | `Placeholder`. Case-insensitive "contains" over the listing's `SearchBy`, after typing pauses for 300 ms; it replaces the history entry instead of adding one. In-memory listings need `SearchBy`; a server `Load` without it gets the text in `query.Search`. |
| `Choice` | `Title`, `By` (`order => order.Status`), `Options` (`Option(Value, Title)`), `Name` (default: the member path of `By`). |
| `Flag`   | `Title`, `Name`, `Where` (`order => order.Total > 1000`). On is `name=1` in the address.                                  |

Two filters with one name fail on the first render. An address value that matches no option is ignored.

The address keys are `page`, `sort`, `q` and the filter names. A hand-edited address never breaks the page: a bad page number opens the first page, a page past the end opens the last one, an unknown sort key falls back to the default sort. A sortable cell without a member path fails on the first render with a message naming the cell.

### Selection and freshness

```razor
<PageHead />
<Listing TItem="Order" Load="Store.PageAsync" Selectable Live>
    <Selected>
        <Button Kind="ButtonKind.Danger" OnClick="() => Store.CancelAsync(context)" Done="Orders cancelled">Cancel</Button>
    </Selected>
    <Columns>...</Columns>
</Listing>
```

| Parameter    | Description                                                                                               |
| ------------ | --------------------------------------------------------------------------------------------------------- |
| `Selectable` | Adds checkboxes. The selection covers the current page and clears when the page, sort or filters change. |
| `Selected`   | A bar above the table while rows are selected; `context` is the selected items.                          |
| `ItemKey`    | What identifies a row, so the selection survives a reload: `order => order.Id`. Default: the item itself. |
| `Live`       | Reloads every `AutoRefreshInterval` while the tab is visible.                                             |

`PageHead` without `Count` and `UpdatedAt` follows the page: it shows the total and the time of the last load of the first listing on the page, and its refresh stamp reloads every listing. An action run through `Button` or `Act` reloads them too.

The first load shows a skeleton; a reload keeps the rows on screen; a failed load shows a message and a retry inside the listing while the rest of the page keeps working. Call `RefreshAsync()` on a `@ref` to reload from code.

## Dialogs

```csharp
if (await Dialogs.ConfirmAsync("Delete the order?", "The customer is notified.", "Delete", danger: true))
    await Store.DeleteAsync(order);

var sheet = await Dialogs.ShowSheetAsync<OrderForm>("New order");
var result = await sheet.Result;
```

`ShowSheetAsync` opens a side sheet on desktop and a bottom sheet on phones. With the default `form: true`, a click outside the sheet does not close it. Pass `form: false` for read-only details. A sheet component wraps its markup in `Sheet`, which has `ChildContent` and `Actions`.

## Forms

```razor
<Sheet>
    <Form Model="_order" OnSubmit="SaveAsync" Submit="Create" Done="Order created">
        <Field For="() => _order.Customer" />
        <Field For="() => _order.Total" />
        <Field For="() => _order.Status" />
    </Form>
</Sheet>

@code {
    private readonly OrderDraft _order = new();

    private Task SaveAsync() => Store.CreateAsync(_order);
}
```

```csharp
var order = await Dialogs.ShowFormAsync<OrderSheet, OrderDraft>("New order");
```

`Form` validates with data annotations, shows a busy submit button and submits once on Enter. On success it reports `Done`, reloads the page's blocks, and inside a sheet closes it with the model, which `ShowFormAsync` returns; a closed sheet returns `null`.

- `OnSubmit` throws `FieldException(field, message)` or `FieldException(errors)` for errors next to fields; they clear when the field changes.
- A `PanelException` shows its message above the button; any other exception shows a general message and is logged.
- A changed form asks before its sheet closes (the close button or Escape) and before the page is left.
- Any MudBlazor input with `For=` works inside `Form`, and `Form` works on a page too, for example in settings.

| Parameter        | Description                                                     |
| ---------------- | --------------------------------------------------------------- |
| `Model`        | The edited object.                                              |
| `OnSubmit`     | Called with a valid model.                                      |
| `Submit`       | Text of the submit button. Default "Save".                      |
| `Done`         | Message after success. Default "Saved".                         |
| `Refresh`      | Reloads the page's blocks after success. Default `true`.      |
| `Actions`      | More buttons before the submit button.                          |

`Field` picks the input from the property type and takes its label from `[Display(Name)]` (resources included), its hint from `[Display(Description)]` and its required mark from `[Required]`.

| Type                                    | Input                                                    |
| --------------------------------------- | -------------------------------------------------------- |
| `string`                              | Text; `Lines` for several lines, `Secret` for a password. |
| Numbers                                 | Numeric field.                                           |
| `bool`, `bool?`                     | Switch, three-state checkbox.                            |
| Enums, or any type with `Options`     | Select with the `[Display(Name)]` of each member.       |
| `DateOnly`                            | Date.                                                    |
| `DateTime`, `DateTimeOffset`        | Date and time in the viewer's time zone, saved in UTC.  |

| Parameter        | Description                                                     |
| ---------------- | --------------------------------------------------------------- |
| `For`          | `() => _order.Customer`.                                      |
| `Label`, `Hint` | Override the attributes.                                     |
| `Options`      | `Option(Value, Title)` choices, for example for a `string`. |
| `Lines`, `Secret`, `Disabled` | Text input settings.                            |

Another type fails with a message that points to MudBlazor inputs.

## Components

| Component                    | Description                                                                                                           |
| ---------------------------- | --------------------------------------------------------------------------------------------------------------------- |
| `Facts`, `Fact`           | A record's labelled values: `<Fact Label="Total" Value="order.Total" />`, formatted by `Words.Of`, or own content. |
| `Tag`                      | A status label with `Tone`: `Neutral`, `Good`, `Warn` or `Bad`.                                        |
| `Tabs`, `Tab`              | Tabs whose choice lives in the address (`tab=`, or `Name`). A `Tab` has `Title` and optional `Name`; only the open tab renders. |
| `Timeline`                 | Events from `TimelineEntry(At, Text, Who, Tone)`, newest first, in the viewer's time zone.              |
| `Pane`                     | A titled section of a page with `Hint` and `Actions`.                                                    |
| `Setting`                  | A settings row: `Title`, `Hint` and the control on the right. Group rows in `<div class="settings">`. |
| `EmptyState`               | An empty state: `Title`, `Hint` and an optional action.                                                  |
| `PageHead`                 | Page title with `Count`, `Summary`, `UpdatedAt` with `OnRefresh`, and `Actions`. Without `Count` and `UpdatedAt` it follows the page's listings. |
| `Placeholder`              | Skeleton for content that is still loading.                                                                           |
| `AutoRefresh`              | Invokes `OnTick` every `AutoRefreshInterval` while the tab is visible.                                            |
| `Sheet`                    | Layout for a sheet: `ChildContent` and `Actions`.                                                                 |
| `Figure`                   | Large number with two caption lines (`First`, `Second`). `Alarm` marks it red. `Change` (a share: `0.12` is +12%) shows the change to the previous period, good when it grows unless `LowerIsBetter`. Group in `<div class="figs">`. |
| `Chart`                    | A line or bar chart over time from `Trend(Name, Readings)` of `Reading(At, Value)`. See below.                        |
| `Strips`                   | Horizontal bars for a ranking, from `Strip(Label, Value, Shown)`.                                                   |
| `Meter`                    | `Used / Limit` with a bar that turns ochre at 90% and red at 100%.                                                  |
| `Crumb`                    | Removable filter chip that links to the address without the filter.                                                   |
| `CopyLine`, `CopyButton` | A value with a copy button.                                                                                           |
| `KeyField`                 | A secret token in groups, with a copy button.                                                                         |
| `EntranceLayout`           | Full-screen layout without navigation, for sign-in pages: `@layout EntranceLayout`.                                 |
| `LanguageLink`             | Link to the next language, for pages outside the shell.                                                               |

```razor
<Chart Trends="[new Trend("Orders", _orders), new Trend("Refunds", _refunds)]" Step="Period.Day" From="_monthAgo" To="_now" />
```

| Parameter         | Description                                                                                  |
| ----------------- | -------------------------------------------------------------------------------------------- |
| `Trends`        | Series of readings. Readings in one bucket are summed; empty buckets are zero.              |
| `Kind`          | `Line` (default) or `Bars`.                                                                |
| `Step`          | `Hour`, `Day` (default), `Week` (from Monday) or `Month`, bounded in the viewer's time zone. |
| `From`, `To`  | Extend the axis beyond the readings.                                                         |
| `Format`        | Axis values. Default: whole numbers by `Words.Number`.                                     |
| `Height`        | Pixels. Default `240`.                                                                     |

MudBlazor inputs, selects, switches, checkboxes, tables, menus and snackbars are themed.

## Formatting

`Words` formats values consistently in the current culture.

| Method                         | Example                                                                    |
| ------------------------------ | -------------------------------------------------------------------------- |
| `Number(1284)`               | `1,284`                                                                  |
| `Size(1536)`                 | `1.5 KB`                                                                 |
| `Duration(1240)`             | `1.2 s`                                                                  |
| `Share(0.123)`               | `12.3%`                                                                  |
| `Of(value)`                   | Any of the above by type, `–` for `null`                                |
| `Day`, `Minute`, `Clock` | `2026-10-26`, `2026-10-26 14:05`, `14:05` in the viewer's time zone |
| `Middle(text, 10)`           | `tnk_AB…XYZ`                                                            |
| `Count(pattern, n)`          | ICU plurals: `{n, plural, one {{number} order} other {{number} orders}}` |

Times are shown in the viewer's time zone, which the browser reports once per connection. A browser that reports no zone or an unknown one gets UTC. Set `TimeZone` to show every viewer the same zone. Keep data in UTC: blocks never change the host's values.

`Clipboard.CopyAsync(text)` copies from code and explains when the page is not served over HTTPS.

## Styling

| Class                                         | Use                                              |
| --------------------------------------------- | ------------------------------------------------ |
| `btn-k pigment`                             | Primary button.                                  |
| `btn-danger`                                | Destructive button.                              |
| `link`                                      | Quiet text button.                               |
| `tbl`, `tbl dense`                        | On `MudTable`. Add `num` to numeric columns. |
| `empty`                                     | Empty state; `EmptyState` renders it.          |
| `block`, `lead`                           | Titled section further down a page; `Pane` replaces them. |
| `group`, `pair`, `hint`, `form-error` | Form layout inside a sheet.                      |
| `note`, `warn-t`, `bad-t`               | Secondary, warning and error text.               |
| `sr`                                        | Visible to screen readers only.                  |

Design tokens are CSS custom properties: `--night`, `--deep`, `--rule`, `--mist`, `--light`, `--klein`, `--glow`, `--vermilion`, `--ochre`, `--sans`, `--mono`. With `Appearance` set to `Light` (or `System` on a light device) they keep their names and take light values: `--night` becomes the light background and `--light` the dark text.

## Authentication

Enfilade uses the host's authentication and does not provide its own.

- Without `LoginPath`, every page is public.
- With `LoginPath`, every page and endpoint requires an authenticated user, and anonymous visitors are redirected to `LoginPath`. Mark pages that stay public, including the sign-in page, with `[AllowAnonymous]`.
- `SignOutPath` adds a sign-out item under the user name.
- Sections follow their page's `[Authorize(Roles = ...)]` and `[Authorize(Policy = ...)]`: a section the user may not open is left out of the navigation, and a caption with no visible sections after it disappears. The page attribute is the only place rights are declared.
- Opening such a page directly shows "No access" inside the panel to a signed-in user, and sends an anonymous visitor to `LoginPath`.

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
| `Appearance`          | `Dark`     | `Dark`, `Light`, or `System` to follow the device.                                  |
| `Accent`              | —           | Brand color in place of Klein, as `#RRGGBB`.                                        |
| `TimeZone`            | —           | Time zone id for every viewer, such as `UTC` or `Europe/Berlin`. Without it, each viewer sees their own zone. |
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
