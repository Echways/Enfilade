# Аутентификация

[English](../en/authentication.md) · [Русский](authentication.md) · [Документация](README.md)

Enfilade использует аутентификацию хоста и своей не предоставляет. Cookie, OpenID Connect, Identity — то, что уже работает в хосте.

- Без `LoginPath` все страницы открыты.
- С `LoginPath` каждая страница и каждый эндпоинт требуют аутентифицированного пользователя, а анонимные посетители перенаправляются на `LoginPath`. Страницы, которые остаются открытыми, включая саму страницу входа, помечайте `[AllowAnonymous]`.
- `SignOutPath` добавляет пункт выхода под именем пользователя.
- Разделы следуют `[Authorize(Roles = ...)]` и `[Authorize(Policy = ...)]` своей страницы: раздел, который пользователю открывать нельзя, не попадает в навигацию, а заголовок группы без видимых разделов после него исчезает. Атрибут страницы — единственное место, где объявляются права.
- Если такую страницу открыть напрямую, вошедший пользователь увидит внутри панели «Нет доступа», а анонимный посетитель попадёт на `LoginPath`.

## Пример

```csharp
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie();
builder.Services.AddAuthorization();

builder.Services.AddEnfilade(shell =>
    {
        shell.LoginPath = "login";
        shell.SignOutPath = "logout";
    })
    .AddSection<Orders>("Orders")
    .AddSection<Users>("Users");

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.MapEnfilade();
```

```razor
@page "/users"
@attribute [Authorize(Roles = "Admin")]
```

Раздел `Users` теперь виден только администраторам. Для остальных его нет в меню, а прямая ссылка показывает «Нет доступа».

Страница входа — обычная страница хоста. `@layout EntranceLayout` даёт полноэкранный вид без навигации, а `[AllowAnonymous]` не даёт перенаправлению зациклиться:

```razor
@page "/login"
@layout EntranceLayout
@attribute [AllowAnonymous]
```

`MapEnfilade` добавляет в конвейер antiforgery и локализацию запросов, поэтому вызывайте его после middleware аутентификации.
