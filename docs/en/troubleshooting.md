# Troubleshooting

[English](troubleshooting.md) · [Русский](../ru/troubleshooting.md) · [Documentation](README.md)

Enfilade checks what it can at startup or on the first render and names the cause in the message. This page lists the most common ones.

## At startup

| Message points to | Cause | Fix |
| ----------------- | ----- | --- |
| `Language` | The default language is not among `Languages`. | Add it to `Languages` or change `Language`. |
| `TimeZone` | The host does not know the time zone id. | Use an id from `TimeZoneInfo.GetSystemTimeZones()`, such as `Europe/Berlin`. On a slim container image, install `tzdata`. |
| `Accent` | Not a `#RRGGBB` color. | Pass six hex digits with the `#`. |
| `AutoRefreshInterval` | Zero or negative. | Use a positive interval. |
| "has no @page route without parameters" | The page of a section only has routes with parameters. | Add a route without parameters, or register the page with `AddPages` and link to it yourself. |

## On the first render of a page

| Situation | Fix |
| --------- | --- |
| A `Listing` has both `Load` and `Items`, or neither. | Pass exactly one. |
| A sortable `Cell` has no member path (a computed `Value`). | Add `Key`. |
| Two filters of a listing share a name. | Give each a distinct `Name`. |
| A selectable listing with `Load` and items that are not records. | Add `ItemKey`. |
| `Field` over a type it does not know. | Use a MudBlazor input with `For=` inside the `Form`. |
| Two `Tabs` on one page. | Give each a distinct `Name`. |
| Two `Listing`s on one page. | Give each a distinct `Name`, so the address keys do not collide. |

## Behavior

**The section is missing from the menu.** Its page has an `[Authorize(Roles = ...)]` or `[Authorize(Policy = ...)]` the current user does not satisfy. That is the intended result.

**Everyone is redirected to the sign-in page, in a loop.** The sign-in page itself needs `[AllowAnonymous]` once `LoginPath` is set.

**A page opened from a link shows "No access".** The user is signed in but fails the page's role or policy. That is the intended result.

**The copy button says it cannot copy.** Clipboard access needs HTTPS (or `localhost`). Serve the panel over HTTPS.

**Times are an hour off.** Keep stored values in UTC. The viewer's zone is reported by the browser once per connection; without a report the panel uses UTC. Set `TimeZone` to show one zone to everyone.

**Authentication does not apply to the panel.** `MapEnfilade` must come after `UseAuthentication` and `UseAuthorization`.

**A badge is missing.** A count of zero shows nothing. A count that fails is logged and skipped; look for it in the log.

**An exception text is not shown to the user.** Only `PanelException` and exceptions registered with `ShowMessageOf` are shown. Everything else gets a general message and is logged. See [Error messages](errors.md).
