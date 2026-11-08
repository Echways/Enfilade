namespace Enfilade.Tests;

public class NoticeTests : PanelContext
{
    [Theory]
    [InlineData(Tone.Neutral, "neutral", "status")]
    [InlineData(Tone.Good, "good", "status")]
    [InlineData(Tone.Warn, "warn", "alert")]
    [InlineData(Tone.Bad, "bad", "alert")]
    public void Tone_picks_the_class_and_how_it_is_announced(Tone tone, string expectedClass, string expectedRole)
    {
        var notice = Render<Notice>(parameters => parameters.Add(notice => notice.Tone, tone));

        var element = notice.Find(".notice");
        Assert.Contains(expectedClass, element.GetAttribute("class"));
        Assert.Equal(expectedRole, element.GetAttribute("role"));
    }

    [Fact]
    public void Title_content_and_actions_are_shown()
    {
        var notice = Render<Notice>(parameters => parameters
            .Add(notice => notice.Title, "Лицензия истекает")
            .AddChildContent("через 3 дня")
            .Add(notice => notice.Actions, builder => builder.AddContent(0, "Продлить")));

        Assert.Equal("Лицензия истекает", notice.Find("strong").TextContent);
        Assert.Contains("через 3 дня", notice.Find(".notice-text").TextContent);
        Assert.Equal("Продлить", notice.Find(".actions").TextContent);
    }

    [Fact]
    public void Without_actions_no_actions_box_is_rendered()
    {
        var notice = Render<Notice>(parameters => parameters.AddChildContent("текст"));

        Assert.Empty(notice.FindAll(".actions"));
        Assert.Empty(notice.FindAll("strong"));
    }
}
