# Authentication

[English](authentication.md) · [Русский](../ru/authentication.md) · [Documentation](README.md)

Enfilade uses the host's authentication and does not provide its own. Cookies, OpenID Connect, Identity: whatever the host already runs.

- Without `LoginPath`, every page is public.
- With `LoginPath`, every page and endpoint requires an authenticated user, and anonymous visitors are redirected to `LoginPath`. Mark pages that stay public, including the sign-in page, with `[AllowAnonymous]`.
- `SignOutPath` adds a sign-out item under the user name.
- Sections follow their page's `[Authorize(Roles = ...)]` and `[Authorize(Policy = ...)]`: a section the user may not open is left out of the navigation, and a caption with no visible sections after it disappears. The page attribute is the only place rights are declared.
- Opening such a page directly shows "No access" inside the panel to a signed-in user, and sends an anonymous visitor to `LoginPath`.

## Example

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

The `Users` section is now visible to administrators only. For everyone else it is absent from the menu, and a direct link shows "No access".

The sign-in page is an ordinary page of the host. Use `@layout EntranceLayout` for the full-screen look without navigation, and `[AllowAnonymous]` so the redirect does not loop:

```razor
@page "/login"
@layout EntranceLayout
@attribute [AllowAnonymous]
```

`MapEnfilade` adds antiforgery and request localization to the pipeline, so call it after authentication middleware.
