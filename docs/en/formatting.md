# Formatting

[English](formatting.md) · [Русский](../ru/formatting.md) · [Documentation](README.md)

`Words` formats values consistently in the current culture.

| Method                         | Example                                                                    |
| ------------------------------ | -------------------------------------------------------------------------- |
| `Number(1284)`               | `1,284`                                                                  |
| `Size(1536)`                 | `1.5 KB`                                                                 |
| `Duration(1240)`             | `1.2 s`                                                                  |
| `Share(0.123)`               | `12.3%`                                                                  |
| `Middle(text, 10)`           | `tnk_AB…XYZ`                                                            |
| `Count(pattern, n)`          | ICU plurals: `{n, plural, one {{number} order} other {{number} orders}}` |
| `Of(value, zone)`             | Any of these by type, times in `zone`, `–` for `null`                   |
| `Day`, `Minute`, `Clock`  | `2026-10-26`, `2026-10-26 14:05`, `14:05` in the given zone             |

## The viewer's time

`ViewerTime` is the viewer's side of the same methods. It is a service of the connection, so a page or a component injects it, and its time zone holds wherever the render starts, including updates pushed from background work.

```razor
@inject ViewerTime Viewer

<span>@Viewer.Minute(order.CreatedAt)</span>
```

| Member                         | Use                                                                      |
| ------------------------------ | ------------------------------------------------------------------------ |
| `Of(value)`                   | `Words.Of` in the viewer's time zone                                    |
| `Day`, `Minute`, `Clock`  | `2026-10-26`, `2026-10-26 14:05`, `14:05` in the viewer's time zone   |
| `ToViewer`, `FromViewer`    | A UTC moment to the viewer's wall clock and back                         |
| `Zone`                        | The viewer's `TimeZoneInfo`                                             |

Times are shown in the viewer's time zone, which the browser reports once per connection. A browser that reports no zone or an unknown one gets UTC. Set `TimeZone` to show every viewer the same zone. Keep data in UTC: blocks never change the host's values.

## Plurals

`Count` takes an ICU message pattern, so the right form is chosen in every language that has plural rules:

```csharp
Words.Count("{n, plural, one {{number} order} other {{number} orders}}", orders.Count)
```

## Clipboard

`Clipboard.CopyAsync(text)` copies from code and explains when the page is not served over HTTPS.
