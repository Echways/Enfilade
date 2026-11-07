# Configuration

[English](configuration.md) · [Русский](../ru/configuration.md) · [Documentation](README.md)

Options bind from the `Enfilade` configuration section. Code passed to `AddEnfilade` takes precedence. Invalid options fail at startup.

```json
{
  "Enfilade": {
    "Brand": "Orders",
    "LoginPath": "login",
    "Languages": ["en", "ru"]
  }
}
```

```csharp
builder.Services.AddEnfilade(shell => shell.Appearance = Appearance.System);
```

## Options

| Option                  | Default      | Description                                     |
| ----------------------- | ------------ | ----------------------------------------------- |
| `Brand`               | `Enfilade` | Name in the navigation and document titles.     |
| `Favicon`             | —           | Static asset path of the icon.                  |
| `Language`            | `en`       | Default language. Must be one of `Languages`. |
| `Languages`           | —           | Languages offered in the switch.                |
| `LoginPath`           | —           | Sign-in page. Makes the panel private.          |
| `SignOutPath`         | —           | Sign-out endpoint.                              |
| `Appearance`          | `Dark`     | `Dark`, `Light`, or `System` to follow the device.                                  |
| `Accent`              | —           | Brand color in place of Klein, as `#RRGGBB`.                                        |
| `TimeZone`            | —           | Time zone id for every viewer, such as `UTC` or `Europe/Berlin`. Without it, each viewer sees their own zone. |
| `AutoRefreshInterval` | `00:00:30` | Period of `AutoRefresh`.                      |

## Validation

The options are checked when the application starts, so a typo does not wait for a user to find it:

- `Language` is one of `Languages` (when `Languages` is set).
- `TimeZone` is a time zone the host knows.
- `Accent` is a `#RRGGBB` color.
- `AutoRefreshInterval` is greater than zero.

## Environments

Because options bind from configuration, per-environment overrides work as usual, for example a different `Brand` in `appsettings.Staging.json` so a staging panel is never mistaken for production.
