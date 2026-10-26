using System.Globalization;

namespace Enfilade.Tests;

public class LanguageChoiceTests
{
    [Theory]
    [InlineData("en", "ru")]
    [InlineData("ru", "de")]
    [InlineData("de", "en")]
    public void Switch_offers_the_language_after_the_current_one(string current, string next)
    {
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(current);

        Assert.Equal(next, LanguageChoice.Next(new EnfiladeOptions { Languages = ["en", "ru", "de"] })?.Name);
    }

    [Fact]
    public void Switch_is_hidden_when_the_panel_speaks_one_language() =>
        Assert.Null(LanguageChoice.Next(new EnfiladeOptions { Language = "ru" }));

    [Fact]
    public void Without_a_list_the_panel_speaks_only_its_language() =>
        Assert.Equal(["ru"], LanguageChoice.Spoken(new EnfiladeOptions { Language = "ru" }));

    [Theory]
    [InlineData("ru", "Русский")]
    [InlineData("en", "English")]
    public void Language_is_named_in_itself(string culture, string title) =>
        Assert.Equal(title, LanguageChoice.Title(CultureInfo.GetCultureInfo(culture)));
}
