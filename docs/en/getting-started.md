# Getting started

[English](getting-started.md) · [Русский](../ru/getting-started.md) · [Documentation](README.md)

This page takes an empty Blazor Server project to a working panel with one section.

## Requirements

- .NET 10
- A Blazor Server host (`Microsoft.NET.Sdk.Web`). Enfilade runs on interactive server rendering and brings [MudBlazor](https://mudblazor.com) with it.

## Installation

```shell
dotnet add package Enfilade
```

## Host

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

`AddEnfilade` returns an `EnfiladeBuilder`. Everything else, from sections to error messages, is configured by chaining on it. See [Sections](sections.md).

## The first section

A section is a routable Razor component. Create `Pages/Home.razor`:

```razor
@page "/"

<PageHead />
<Pane Title="Welcome" Hint="Your first section">
    <p>The navigation, the theme and the error handling are already in place.</p>
</Pane>
```

`PageHead` takes the heading from the navigation entry, so the page and its menu item cannot drift apart. It also sets the document title to `Home · Orders`.

## A section with data

The second section is a listing over a store. The store is any service; here it is registered next to the panel:

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

Search, sorting and paging work, the state lives in the address, and the first load shows a skeleton. See [Listings](listings.md) for filters, selection and live reload.

## What to read next

| If you want to | Read |
| -------------- | ---- |
| Register more pages, icons and counters in the menu | [Sections](sections.md) |
| Run actions with confirmation and readable errors | [Actions](actions.md) |
| Build tables with filters, selection and live reload | [Listings](listings.md) |
| Edit records in a side sheet | [Dialogs and forms](forms.md) |
| Put numbers, charts and tabs on a page | [Components](components.md) |
| Close the panel behind a sign-in | [Authentication](authentication.md) |
| Switch languages and the theme | [Localization](localization.md), [Styling](styling.md) |
