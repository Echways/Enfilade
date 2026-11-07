# Error messages

[English](errors.md) · [Русский](../ru/errors.md) · [Documentation](README.md)

`Act`, `Button`, `Form` and `Listing` show the user the message of a `PanelException`. Register other exceptions whose messages are meant for users, such as a domain exception from a library that knows nothing of the panel:

```csharp
builder.Services.AddEnfilade()
    .ShowMessageOf<DomainException>(failure => Words.Sentence(failure.Message));
```

- A registration covers the exception and its subclasses. Registrations are asked in order and the first message wins; returning `null` passes the exception to the next one.
- `ShowMessageOf` also extends `IServiceCollection`, so a library can register its exceptions without the builder. For messages that need services, register an `IErrorMessage` implementation.
- Inject `ErrorMessages` and call `Of(exception)` to get the message in your own components; it returns `null` for an exception the user should not see.
- A shown message is not logged; any other exception is.

## What the user sees

| Exception | Message | Logged |
| --------- | ------- | ------ |
| `PanelException` | Its own text | No |
| A registered exception | The text the registration returns | No |
| Anything else | A general message, with no internals | Yes |

An exception that escapes a page does not take the panel down. See [Runtime behavior](runtime.md).
