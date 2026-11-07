# Библиотеки разделов

[English](../en/libraries.md) · [Русский](libraries.md) · [Документация](README.md)

Раздел может поставляться отдельным пакетом. Библиотека раздела открывает расширение для `EnfiladeBuilder`:

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

Хост вызывает `services.AddEnfilade().AddOrders()`. Изолированный CSS библиотеки подключается автоматически. Её `wwwroot` отдаётся по адресу `_content/{PackageId}/`.

## Рекомендации

- Регистрируйте сервисы библиотеки в том же расширении, чтобы хосту хватало одного вызова.
- Берите заголовок из ресурса через `Func<string>`, чтобы раздел следовал языку зрителя. См. [Локализацию](localization.md).
- Права объявляйте на странице через `[Authorize]`: навигация хоста их учитывает. См. [Аутентификацию](authentication.md).
- Исключения своего домена регистрируйте через `ShowMessageOf` у `IServiceCollection`, чтобы библиотеке не требовался билдер. См. [Сообщения об ошибках](errors.md).
- Глобальные стили подключайте через `AddStylesheet("_content/{PackageId}/library.css")`.
