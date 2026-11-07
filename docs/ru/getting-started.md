# Быстрый старт

[English](../en/getting-started.md) · [Русский](getting-started.md) · [Документация](README.md)

На этой странице пустой проект Blazor Server превращается в работающую панель с одним разделом.

## Требования

- .NET 10
- Хост на Blazor Server (`Microsoft.NET.Sdk.Web`). Enfilade работает на интерактивном серверном рендеринге и приносит с собой [MudBlazor](https://mudblazor.com).

## Установка

```shell
dotnet add package Enfilade
```

## Хост

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

Добавьте пространства имён в `_Imports.razor`:

```razor
@using Enfilade
@using MudBlazor
```

`MapEnfilade` подключает панель, её статические файлы и эндпоинт выбора языка. Заодно он добавляет в конвейер локализацию запросов и antiforgery, поэтому вызывайте его после middleware аутентификации.

`AddEnfilade` возвращает `EnfiladeBuilder`. Всё остальное, от разделов до сообщений об ошибках, настраивается цепочкой вызовов на нём. Подробнее в [Разделах](sections.md).

## Первый раздел

Раздел — это обычный Razor-компонент с маршрутом. Создайте `Pages/Home.razor`:

```razor
@page "/"

<PageHead />
<Pane Title="Welcome" Hint="Your first section">
    <p>The navigation, the theme and the error handling are already in place.</p>
</Pane>
```

`PageHead` берёт заголовок из пункта навигации, поэтому страница и её пункт меню не разойдутся. Он же выставляет заголовок документа: `Home · Orders`.

## Раздел с данными

Второй раздел — таблица над хранилищем. Хранилище — обычный сервис, зарегистрируйте его рядом с панелью:

```csharp
builder.Services.AddScoped<OrderStore>();
```

```razor
@page "/orders"
@inject OrderStore Store

<PageHead />
<Listing TItem="Order" Load="Store.PageAsync" SearchBy="[order => order.Customer]" Sort="-createdAt">
    <Filters>
        <Search />
    </Filters>
    <Columns>
        <Cell Title="Customer" Value="order => order.Customer" Sort />
        <Cell Title="Total" Value="order => order.Total" Sort />
        <Cell Title="Created" Value="order => order.CreatedAt" Sort />
    </Columns>
</Listing>
```

```csharp
public sealed class OrderStore(ShopContext db)
{
    public Task<ListPage<Order>> PageAsync(ListQuery<Order> query, CancellationToken token) =>
        Task.FromResult(db.Orders.ToPage(query));
}
```

Поиск, сортировка и постраничный вывод уже работают, состояние хранится в адресе, а первая загрузка показывает скелетон. Фильтры, выбор строк и живое обновление описаны в [Списках](listings.md).

## Что читать дальше

| Если нужно | Читайте |
| ---------- | ------- |
| Добавить страницы, иконки и счётчики в меню | [Разделы](sections.md) |
| Выполнять действия с подтверждением и понятными ошибками | [Действия](actions.md) |
| Строить таблицы с фильтрами, выбором и живым обновлением | [Списки](listings.md) |
| Редактировать записи в боковой панели | [Диалоги и формы](forms.md) |
| Разместить на странице числа, графики и вкладки | [Компоненты](components.md) |
| Закрыть панель входом | [Аутентификация](authentication.md) |
| Переключать язык и тему | [Локализация](localization.md), [Оформление](styling.md) |
