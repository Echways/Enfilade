# Localization

[English](localization.md) · [Русский](../ru/localization.md) · [Documentation](README.md)

```csharp
builder.Services.AddEnfilade(shell =>
{
    shell.Language = "en";
    shell.Languages = ["en", "ru"];
});
```

With more than one language, the navigation shows a language switch. The choice is stored in the standard ASP.NET Core culture cookie. Built-in strings ship in English and Russian.

`Language` is the default and must be one of `Languages`; otherwise the application fails at startup.

## Your own texts

Titles passed to `AddSection` can be a `Func<string>`, evaluated on each render, so they follow the culture of the current viewer:

```csharp
.AddSection<Orders>(() => Texts.Orders)
.AddAccountLink(() => Texts.Profile, "profile")
.AddCaption(() => Texts.Shop)
```

`Texts` is an ordinary resource file (`Texts.resx`, `Texts.ru.resx`). Field labels and hints come from `[Display(Name = ..., ResourceType = ...)]` the same way.

## Outside the shell

Pages that do not use the shell layout, such as a sign-in page, can still switch the language with `LanguageLink`, which links to the next language in `Languages`.

## Dates and numbers

Numbers and dates follow the current culture; times follow the viewer's time zone. See [Formatting](formatting.md).
