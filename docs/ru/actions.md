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
- После успешного или упавшего действия перезагружается всё на странице, что отчитывается перед `CurrentPage`: ошибка часто значит, что данные изменились у пользователя за спиной, например запись удалили в другом месте. Отказ в подтверждении и отменённое действие страницу не трогают. Чтобы пропустить перезагрузку, передайте `Refresh="false"` (`refresh: false`).
- `RunAsync` возвращает `ActResult`, чтобы вызывающий код различал случаи.

| `ActResult` | Что произошло                                                    | Страница перезагружена |
| ----------- | ---------------------------------------------------------------- | ---------------------- |
| `Done`      | Действие дошло до конца.                                         | Да                     |
| `Declined`  | Пользователь отказался в вопросе `Confirm`, ничего не выполнялось. | Нет                  |
| `Cancelled` | Действие отменено, например пользователь ушёл со страницы.       | Нет                    |
| `Failed`    | Действие выбросило исключение, пользователь об этом уведомлён.   | Да                     |

Подтверждение тоже выполняет `Act`, поэтому спрашивать отдельно и перезагружать вручную не нужно:

```csharp
var result = await Act.RunAsync(
    () => Store.IssueAsync(client),
    confirm: new Confirm("Reissue the key?", "The old key stops working.", "Reissue"));

if (result is ActResult.Done)
    await Dialogs.ShowConnectionAsync(client);
```

## Параметры Button

| Параметр          | Описание                                                                 |
| ----------------- | ------------------------------------------------------------------------ |
| `Kind`          | `Primary` (по умолчанию), `Danger` или `Quiet`.                        |
| `OnClick`       | Действие. Пока оно идёт, кнопка отключена и показывает занятость.       |
| `Confirm`       | `Confirm(Question, Message, Yes, Danger)` — вопрос перед действием.    |
| `Done`          | Сообщение после успеха.                                                  |
| `Refresh`       | Перезагружает блоки страницы после успеха или ошибки. По умолчанию `true`.        |
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
