# Локализация

[English](../en/localization.md) · [Русский](localization.md) · [Документация](README.md)

```csharp
builder.Services.AddEnfilade(shell =>
{
    shell.Language = "ru";
    shell.Languages = ["ru", "en"];
});
```

Если языков больше одного, в навигации появляется переключатель языка. Выбор хранится в стандартном cookie культуры ASP.NET Core. Встроенные строки поставляются на английском и русском.

`Language` — язык по умолчанию, он должен входить в `Languages`; иначе приложение не запустится.

## Собственные тексты

Заголовки в `AddSection` могут быть `Func<string>`, вычисляемым при каждом рендере, поэтому они следуют культуре текущего зрителя:

```csharp
.AddSection<Orders>(() => Texts.Orders)
.AddAccountLink(() => Texts.Profile, "profile")
.AddCaption(() => Texts.Shop)
```

`Texts` — обычный файл ресурсов (`Texts.resx`, `Texts.ru.resx`). Подписи и подсказки полей берутся из `[Display(Name = ..., ResourceType = ...)]` тем же способом.

## Вне оболочки

Страницы, которые не используют макет оболочки, например страница входа, всё равно могут переключать язык через `LanguageLink`: он ведёт на следующий язык из `Languages`.

## Даты и числа

Числа и даты следуют текущей культуре, время — часовому поясу зрителя. См. [Форматирование](formatting.md).
