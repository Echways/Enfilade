using MudBlazor;
using MudBlazor.Utilities;

namespace Enfilade.Tests;

public class EnfiladeThemeTests
{
    [Fact]
    public void Accent_becomes_primary_in_both_palettes()
    {
        var theme = EnfiladeTheme.For("#0A7F5A");

        Assert.Equal(new MudColor("#0A7F5A"), theme.PaletteDark.Primary);
        Assert.Equal(new MudColor("#0A7F5A"), theme.PaletteLight.Primary);
    }

    [Fact]
    public void Default_theme_keeps_klein()
    {
        var theme = EnfiladeTheme.For(null);

        Assert.Equal(new MudColor(EnfiladeTheme.Klein), theme.PaletteDark.Primary);
        Assert.Equal(new MudColor(EnfiladeTheme.Klein), theme.PaletteLight.Primary);
    }

    [Fact]
    public void Light_palette_reads_dark_on_light() =>
        Assert.NotEqual(EnfiladeTheme.For(null).PaletteDark.TextPrimary, EnfiladeTheme.For(null).PaletteLight.TextPrimary);
}
