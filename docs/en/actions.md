# Actions

[English](actions.md) · [Русский](../ru/actions.md) · [Documentation](README.md)

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

- A `PanelException(message)` shows its message to the user as is, and so does any exception registered with [`ShowMessageOf`](errors.md). Any other exception shows a general message and is logged.
- After a successful or a failed action, everything on the page that reports to `CurrentPage` reloads: a failure often means the data has changed under the user, such as a record deleted elsewhere. A declined confirmation and a cancelled action leave the page alone. Pass `Refresh="false"` (`refresh: false`) to skip it.
- `RunAsync` returns an `ActResult`, so the caller can tell the cases apart.

| `ActResult` | What happened                                                    | Page reloaded |
| ----------- | ---------------------------------------------------------------- | ------------- |
| `Done`      | The action ran to the end.                                       | Yes           |
| `Declined`  | The user declined the `Confirm` question; nothing ran.           | No            |
| `Cancelled` | The action was cancelled, for example the user left the page.    | No            |
| `Failed`    | The action threw; the user has been told.                        | Yes           |

Confirmation belongs to `Act` too, so there is no need to ask separately and reload by hand:

```csharp
var result = await Act.RunAsync(
    () => Store.IssueAsync(client),
    confirm: new Confirm("Reissue the key?", "The old key stops working.", "Reissue"));

if (result is ActResult.Done)
    await Dialogs.ShowConnectionAsync(client);
```

## Button parameters

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

## Refusing with a message

Throw a `PanelException` from the action when the user has to hear why it did not work:

```csharp
public async Task CancelAsync(Order order)
{
    if (order.Shipped)
        throw new PanelException("A shipped order cannot be cancelled.");

    await Repository.CancelAsync(order);
}
```

The user sees the sentence as is and the page stays untouched. Exceptions from libraries that know nothing about the panel can be shown the same way once they are registered; see [Error messages](errors.md).

## The current page

`CurrentPage` is the scoped service behind refreshing. It connects the things on a page that load data with the things that change it.

- A custom block joins it with `using var source = CurrentPage.Track(ReloadAsync)` and calls `source.Report(count, at)` after each load.
- `PageHead` shows what the first source reported: the total and the time of the last load.
- `RefreshAsync()` reloads every source and then raises `Refreshed`.
- `Changed` fires when a report arrives.

`Listing` is a source out of the box, and so are the badges in the navigation. A block of your own, such as a card with live numbers, does the same in a few lines:

```razor
@implements IDisposable
@inject CurrentPage CurrentPage
@inject OrderStore Store

<Figure Value="_pending" First="Pending" Second="orders" />

@code {
    private PageSource? _source;
    private long _pending;

    protected override async Task OnInitializedAsync()
    {
        _source = CurrentPage.Track(LoadAsync);
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        _pending = await Store.CountPendingAsync();
        _source?.Report(_pending, DateTimeOffset.UtcNow);
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose() => _source?.Dispose();
}
```
