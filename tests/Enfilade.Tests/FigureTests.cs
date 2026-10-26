namespace Enfilade.Tests;

public class FigureTests : PanelContext
{
    [Fact]
    public void Growth_is_good_by_default()
    {
        var change = Change(0.12);

        Assert.Contains("good", change.GetAttribute("class"));
        Assert.Equal("+" + Words.Share(0.12), change.QuerySelector("[aria-hidden]")!.TextContent);
        Assert.Equal(string.Format(EnfiladeText.ChangeToPrevious, "+" + Words.Share(0.12)), change.QuerySelector(".sr")!.TextContent);
    }

    [Fact]
    public void Drop_is_bad_by_default()
    {
        var change = Change(-0.04);

        Assert.Contains("bad", change.GetAttribute("class"));
        Assert.Equal("−" + Words.Share(0.04), change.QuerySelector("[aria-hidden]")!.TextContent);
    }

    [Fact]
    public void Lower_is_better_swaps_the_tone()
    {
        Assert.Contains("good", Change(-0.04, lowerIsBetter: true).GetAttribute("class"));
        Assert.Contains("bad", Change(0.12, lowerIsBetter: true).GetAttribute("class"));
    }

    [Fact]
    public void No_change_is_neutral()
    {
        var change = Change(0);

        Assert.Contains("neutral", change.GetAttribute("class"));
        Assert.Equal(Words.Share(0), change.QuerySelector("[aria-hidden]")!.TextContent);
    }

    [Fact]
    public void Figure_without_change_shows_none() =>
        Assert.Empty(Render<Figure>(parameters => parameters.Add(figure => figure.Value, 5).Add(figure => figure.First, "заказов").Add(figure => figure.Second, "сегодня")).FindAll(".change"));

    [Fact]
    public void Value_counts_its_characters_without_separators() =>
        Assert.Equal("--chars: 5", Render<Figure>(parameters => parameters.Add(figure => figure.Value, 48200).Add(figure => figure.First, "заказов").Add(figure => figure.Second, "за неделю")).Find("b").GetAttribute("style"));

    [Fact]
    public void Text_counts_its_letters() =>
        Assert.Equal("--chars: 9", Render<Figure>(parameters => parameters.Add(figure => figure.Text, "Отключено").Add(figure => figure.First, "связь").Add(figure => figure.Second, "с банком")).Find("b").GetAttribute("style"));

    private AngleSharp.Dom.IElement Change(double share, bool lowerIsBetter = false) => Render<Figure>(parameters => parameters
        .Add(figure => figure.Value, 1284)
        .Add(figure => figure.First, "заказов")
        .Add(figure => figure.Second, "за неделю")
        .Add(figure => figure.Change, share)
        .Add(figure => figure.LowerIsBetter, lowerIsBetter)).Find(".change");
}
