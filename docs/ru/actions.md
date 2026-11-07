# Действия

[English](../en/actions.md) · [Русский](actions.md) · [Документация](README.md)

`Button` выполняет действие через `Act`: спрашивает подтверждение, блокирует повторные нажатия, пока действие идёт, сообщает об успехе и показывает понятную ошибку.

```razor
<Button Kind="ButtonKind.Danger" OnClick="() => Store.DeleteAsync(order)" Done="Order deleted"
        Confirm="@(new Confirm("Delete the order?", "The customer is notified.", "Delete", Danger: true))">
    Delete
</Button>
```

Из кода внедрите `Act`:

```csharp
await Act.RunAsync(() => Store.DeleteAsync(order), done: "Order deleted");
```

- `PanelException(message)` показывает своё сообщение пользователю как есть. Так же ведёт себя любое исключение, зарегистрированное через [`ShowMessageOf`](errors.md). Остальные исключения показывают общее сообщение и попадают в лог.
- После успешного действия перезагружается всё на странице, что отчитывается перед `CurrentPage`. Чтобы пропустить это, передайте `Refresh="false"` (`refresh: false`).
- `RunAsync` возвращает, дошло ли действие до конца.

## Параметры Button

| Параметр          | Описание                                                                 |
| ----------------- | ------------------------------------------------------------------------ |
| `Kind`          | `Primary` (по умолчанию), `Danger` или `Quiet`.                        |
| `OnClick`       | Действие. Пока оно идёт, кнопка отключена и показывает занятость.       |
| `Confirm`       | `Confirm(Question, Message, Yes, Danger)` — вопрос перед действием.    |
| `Done`          | Сообщение после успеха.                                                  |
| `Refresh`       | Перезагружает блоки страницы после успеха. По умолчанию `true`.        |
| `Submit`        | Рисует кнопку отправки и оставляет работу форме.                        |
| `Busy`, `Disabled` | Показывают кнопку занятой или отключённой снаружи.                  |

Остальные атрибуты, например `class` или `aria-label`, передаются в `<button>`.

## Отказ с сообщением

Если пользователю нужно объяснить, почему не получилось, бросьте из действия `PanelException`:

```csharp
public async Task CancelAsync(Order order)
{
    if (order.Shipped)
        throw new PanelException("A shipped order cannot be cancelled.");

    await Repository.CancelAsync(order);
}
```

Пользователь увидит фразу как есть, страница останется нетронутой. Исключения из библиотек, которые ничего не знают о панели, показываются так же после регистрации; см. [Сообщения об ошибках](errors.md).

## Текущая страница

`CurrentPage` — scoped-сервис, на котором держится обновление. Он связывает то, что на странице загружает данные, с тем, что их меняет.

- Собственный блок подключается так: `using var source = CurrentPage.Track(ReloadAsync)`, а после каждой загрузки вызывает `source.Report(count, at)`.
- `PageHead` показывает то, о чём отчитался первый источник: итог и время последней загрузки.
- `RefreshAsync()` перезагружает все источники и затем поднимает `Refreshed`.
- `Changed` срабатывает, когда приходит отчёт.

`Listing` и счётчики в навигации уже являются источниками. Собственный блок, например карточка с живыми числами, делает то же самое в несколько строк:

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
