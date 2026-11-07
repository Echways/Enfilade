# Section libraries

[English](libraries.md) · [Русский](../ru/libraries.md) · [Documentation](README.md)

A section can ship as a separate package. A section library exposes an extension on `EnfiladeBuilder`:

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

## Guidelines

- Register the library's services in the same extension, so the host makes one call.
- Take the title from a resource through a `Func<string>`, so the section follows the viewer's language. See [Localization](localization.md).
- Declare rights on the page with `[Authorize]`; the host's navigation honors them. See [Authentication](authentication.md).
- Register exceptions of your domain with `ShowMessageOf` on `IServiceCollection`, so the library needs no builder for it. See [Error messages](errors.md).
- Link global styles with `AddStylesheet("_content/{PackageId}/library.css")`.
