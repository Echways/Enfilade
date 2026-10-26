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

    private AngleSharp.Dom.IElement Change(double share, bool lowerIsBetter = false) => Render<Figure>(parameters => parameters
        .Add(figure => figure.Value, 1284)
        .Add(figure => figure.First, "заказов")
        .Add(figure => figure.Second, "за неделю")
        .Add(figure => figure.Change, share)
        .Add(figure => figure.LowerIsBetter, lowerIsBetter)).Find(".change");
}
