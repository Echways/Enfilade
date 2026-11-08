namespace Enfilade.Tests;

public class BannerTests : PanelContext
{
    [Fact]
    public void Banner_is_a_notice_with_the_same_tone_and_content()
    {
        var banner = Render<Banner>(parameters => parameters
            .Add(banner => banner.Tone, Tone.Warn)
            .Add(banner => banner.Title, "Плановые работы")
            .AddChildContent("с 02:00 до 04:00"));

        var notice = banner.Find(".banner > .notice");
        Assert.Contains("warn", notice.GetAttribute("class"));
        Assert.Equal("alert", notice.GetAttribute("role"));
        Assert.Equal("Плановые работы", banner.Find("strong").TextContent);
        Assert.Contains("с 02:00 до 04:00", banner.Find(".notice-text").TextContent);
    }

    [Fact]
    public void Without_actions_and_dismissing_no_actions_box_is_rendered()
    {
        var banner = Render<Banner>(parameters => parameters.AddChildContent("текст"));

        Assert.Empty(banner.FindAll(".actions"));
    }

    [Fact]
    public void Actions_are_shown_next_to_the_close_button()
    {
        var banner = Render<Banner>(parameters => parameters
            .Add(banner => banner.Dismissible, true)
            .Add(banner => banner.Actions, builder => builder.AddContent(0, "Подробнее")));

        var actions = banner.Find(".actions");
        Assert.Contains("Подробнее", actions.TextContent);
        Assert.Single(actions.QuerySelectorAll("button"));
    }

    [Fact]
    public void Close_button_is_only_there_when_the_banner_is_dismissible()
    {
        var fixedBanner = Render<Banner>(parameters => parameters.AddChildContent("текст"));
        var closable = Render<Banner>(parameters => parameters.Add(banner => banner.Dismissible, true));

        Assert.Empty(fixedBanner.FindAll("button"));
        Assert.Equal(EnfiladeText.Close, closable.Find("button").GetAttribute("aria-label"));
    }

    [Fact]
    public void Dismissing_hides_the_banner_and_tells_the_host()
    {
        var dismissed = 0;
        var banner = Render<Banner>(parameters => parameters
            .Add(banner => banner.Dismissible, true)
            .Add(banner => banner.OnDismiss, () => dismissed++));

        banner.Find("button").Click();

        Assert.Empty(banner.FindAll(".banner"));
        Assert.Equal(1, dismissed);
    }
}
