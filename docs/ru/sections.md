# Разделы

[English](../en/sections.md) · [Русский](sections.md) · [Документация](README.md)

Раздел — это Razor-компонент с маршрутом. В навигации разделы идут в порядке регистрации.

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

`OrderSheet` — это `Sheet` с `Form` из `Field`; см. [Диалоги и формы](forms.md).

- `PageHead` берёт заголовок из навигации и выставляет заголовок документа.
- Сборка раздела добавляется в роутер, поэтому вложенные маршруты вроде `/orders/{id}` регистрировать не нужно. Для сборки без разделов есть `AddPages(assembly)`.
- Изолированный CSS (`Orders.razor.css`) подключается автоматически. Глобальные стили добавляются через `AddStylesheet(path)`.
- `NavigationManager.NotFound()` показывает страницу 404 панели.

## Иконки и счётчики

```csharp
.AddSection<Orders>("Orders", section => section
    .Icon(Icons.Material.Outlined.ShoppingCart)
    .Badge<OrderStore>(store => store.CountPendingAsync()))
```

- `Icon` принимает SVG-путь, например иконки MudBlazor.
- Счётчик пересчитывается при переходе, после действия через `Button`, `Act` или `Form` (любой `CurrentPage.RefreshAsync()`) и каждые `AutoRefreshInterval`, пока вкладка видна. Ноль не показывается.
- Ошибка подсчёта попадает в лог и не трогает ни навигацию, ни остальные счётчики.
- Счётчики считаются по очереди, поэтому могут делить один scoped `DbContext`.

## Методы билдера

| Метод                                     | Описание                                                                       |
| ----------------------------------------- | ------------------------------------------------------------------------------ |
| `AddSection<TPage>(string title)`       | Добавляет пункт навигации. У страницы должен быть маршрут `@page` без параметров. |
| `AddSection<TPage>(Func<string> title)` | То же, но заголовок вычисляется при каждом рендере: для локализованных названий. |
| `AddSection<TPage>(title, section => ...)` | То же, с `section.Icon(svg)` и `section.Badge<TService>(service => service.CountAsync())`. |
| `AddAccountLink(title, href)`           | Добавляет в меню аккаунта ссылку, например «Профиль» или «API-ключи»; `title` может быть `Func<string>`. |
| `AddCaption(string text)`               | Добавляет заголовок над следующими за ним разделами.                           |
| `AddPages(Assembly assembly)`           | Добавляет маршруты сборки без пункта навигации.                                |
| `AddStylesheet(string path)`            | Подключает стили из статических файлов.                                        |
| `ShowMessageOf<TException>(message)`    | Показывает пользователю сообщение исключения; см. [Сообщения об ошибках](errors.md). |

## Заголовки групп и ссылки аккаунта

Заголовки делят длинное меню на группы. Права следуют за страницами: заголовок, после которого нет ни одного видимого раздела, исчезает; см. [Аутентификацию](authentication.md).

```csharp
builder.Services.AddEnfilade()
    .AddSection<Home>("Home")
    .AddCaption("Shop")
    .AddSection<Orders>("Orders")
    .AddSection<Customers>("Customers")
    .AddCaption("Account")
    .AddAccountLink("Profile", "profile");
```

## Разделы в отдельных библиотеках

Раздел может поставляться отдельным пакетом и регистрироваться одним вызовом. См. [Библиотеки разделов](libraries.md).
