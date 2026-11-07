# Runtime behavior

[English](runtime.md) · [Русский](../ru/runtime.md) · [Documentation](README.md)

- An exception in a page is contained: the navigation keeps working and the page offers a retry. The page recovers on navigation.
- When the connection drops, a reconnection dialog shows the next attempt. Paused circuits can be resumed.
- After navigation, focus moves to the page heading.
- Layouts honor safe-area insets, coarse pointers and `prefers-reduced-motion`.

## Freshness

Blocks that load data reload at three moments:

1. After an action through `Button`, `Act` or `Form`.
2. When the user presses the refresh stamp in `PageHead`.
3. Every `AutoRefreshInterval`, for `Listing` with `Live`, for `AutoRefresh`, and for the counters in the navigation, only while the browser tab is visible.

A reload keeps what is on screen and replaces it when the data arrives; a failed reload says so inside the block and leaves the rest of the page alone. See [Actions](actions.md) for `CurrentPage`.

## Failures

A load that fails in a listing, a badge that cannot count, and an action that throws are all contained where they happen. The first two never reach the rest of the page; the last one is shown to the user as described in [Error messages](errors.md).
