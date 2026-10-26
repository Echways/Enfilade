using Enfilade.Tests.Hosts;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Enfilade.Tests;

public class PageBlockTests : PanelContext
{
    private NavigationManager Navigation => Services.GetRequiredService<NavigationManager>();

    [Fact]
    public void Fact_formats_its_value()
    {
        var number = Render<Fact>(parameters => parameters.Add(fact => fact.Label, "Сумма").Add(fact => fact.Value, 1234L));
        var nothing = Render<Fact>(parameters => parameters.Add(fact => fact.Label, "Сумма").Add(fact => fact.Value, null));

        Assert.Equal("Сумма", number.Find("dt").TextContent);
        Assert.Equal(Words.Number(1234), number.Find("dd").TextContent);
        Assert.Equal("–", nothing.Find("dd").TextContent);
    }

    [Fact]
    public void Fact_content_wins_over_the_value()
    {
        var fact = Render<Fact>(parameters => parameters
            .Add(fact => fact.Label, "Клиент")
            .Add(fact => fact.Value, 5)
            .AddChildContent("<a href=\"clients/5\">Анна</a>"));

        Assert.Equal("Анна", fact.Find("dd a").TextContent);
    }

    [Fact]
    public void Facts_make_a_description_list() =>
        Assert.Equal("DL", Render<Facts>(parameters => parameters.AddChildContent("<div></div>")).Find(".facts").TagName);

    [Theory]
    [InlineData(Tone.Neutral, "tag neutral")]
    [InlineData(Tone.Good, "tag good")]
    [InlineData(Tone.Warn, "tag warn")]
    [InlineData(Tone.Bad, "tag bad")]
    public void Tag_tone_sets_its_class(Tone tone, string classes) =>
        Assert.Equal(classes, Render<Tag>(parameters => parameters.Add(tag => tag.Tone, tone).AddChildContent("Оплачен")).Find("span").GetAttribute("class"));

    [Fact]
    public void Tabs_render_only_the_active_tab()
    {
        var tabs = Render<TwoTabs>();

        tabs.WaitForElement(".overview");
        Assert.Empty(tabs.FindAll(".history"));

        tabs.FindAll(".mud-tab")[1].Click();

        Assert.EndsWith("?tab=1", Navigation.Uri);
        tabs.WaitForElement(".history");
        Assert.Empty(tabs.FindAll(".overview"));
    }

    [Fact]
    public void Tab_from_the_address_opens()
    {
        Navigation.NavigateTo("?tab=history");

        var tabs = Render<TwoTabs>(parameters => parameters.Add(host => host.SecondName, "history"));

        tabs.WaitForElement(".history");
        Assert.Empty(tabs.FindAll(".overview"));
    }

    [Fact]
    public void Unknown_tab_in_the_address_opens_the_first()
    {
        Navigation.NavigateTo("?tab=nosuchtab");

        var tabs = Render<TwoTabs>();

        tabs.WaitForElement(".overview");
    }

    [Fact]
    public void Back_in_history_switches_the_tab()
    {
        var tabs = Render<TwoTabs>();
        tabs.WaitForElement(".overview");

        Navigation.NavigateTo("?tab=1");

        tabs.WaitForElement(".history");
    }

    [Fact]
    public void Timeline_shows_newest_first_in_the_viewer_zone()
    {
        Words.Zone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Yekaterinburg");
        var older = new TimelineEntry(new DateTimeOffset(2026, 10, 1, 9, 0, 0, TimeSpan.Zero), "Создан", "Анна");
        var newer = new TimelineEntry(new DateTimeOffset(2026, 10, 2, 22, 30, 0, TimeSpan.Zero), "Оплачен", Tone: Tone.Good);

        var timeline = Render<Timeline>(parameters => parameters.Add(timeline => timeline.Entries, [older, newer]));

        var entries = timeline.FindAll("ol > li");
        Assert.Contains("Оплачен", entries[0].TextContent);
        Assert.Contains("2026-10-03 03:30", entries[0].TextContent);
        Assert.Contains("good", entries[0].GetAttribute("class"));
        Assert.Contains("Анна", entries[1].TextContent);
        Assert.Equal("2026-10-02T22:30:00.0000000+00:00", entries[0].QuerySelector("time")!.GetAttribute("datetime"));
    }

    [Fact]
    public void Empty_state_renders_title_hint_and_action()
    {
        var empty = Render<EmptyState>(parameters => parameters
            .Add(state => state.Title, "Ключей пока нет")
            .Add(state => state.Hint, "Создайте первый ключ")
            .AddChildContent("<button class=\"first\">Создать</button>"));

        Assert.Equal("Ключей пока нет", empty.Find("h2").TextContent);
        Assert.Contains("Создайте первый ключ", empty.Markup);
        Assert.NotNull(empty.Find(".empty button.first"));
    }

    [Fact]
    public void Pane_has_a_heading_hint_and_actions()
    {
        var pane = Render<Pane>(parameters => parameters
            .Add(pane => pane.Title, "Лимиты")
            .Add(pane => pane.Hint, "В минуту")
            .Add(pane => pane.Actions, "<button class=\"edit\">Изменить</button>")
            .AddChildContent("<p class=\"inside\">60</p>"));

        Assert.Equal("Лимиты", pane.Find("section.pane h2").TextContent);
        Assert.Contains("В минуту", pane.Markup);
        Assert.NotNull(pane.Find(".pane .edit"));
        Assert.NotNull(pane.Find(".pane .inside"));
    }

    [Fact]
    public void Setting_puts_the_control_beside_its_title()
    {
        var setting = Render<Setting>(parameters => parameters
            .Add(setting => setting.Title, "Уведомления")
            .Add(setting => setting.Hint, "О новых заказах")
            .AddChildContent("<input class=\"control\" type=\"checkbox\" />"));

        Assert.Contains("Уведомления", setting.Find(".setting").TextContent);
        Assert.NotNull(setting.Find(".setting-control .control"));
    }
}
