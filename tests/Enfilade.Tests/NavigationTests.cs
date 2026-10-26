using Enfilade.Layout;
using Enfilade.Tests.Hosts;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace Enfilade.Tests;

public class NavigationTests : PanelContext
{
    [Fact]
    public void Hidden_sections_leave_the_navigation()
    {
        AddAuthorization().SetAuthorized("anna");
        new EnfiladeBuilder(Services).AddSection<EveryonePage>("Все").AddSection<AdminsPage>("Админы");

        var navigation = Render<Navigation>();

        navigation.WaitForAssertion(() => Assert.Equal(["Все"], navigation.FindAll("nav a").Select(link => link.TextContent)));
    }

    [Fact]
    public void Allowed_sections_stay()
    {
        AddAuthorization().SetAuthorized("boris").SetRoles("admin");
        new EnfiladeBuilder(Services).AddSection<EveryonePage>("Все").AddSection<AdminsPage>("Админы");

        var navigation = Render<Navigation>();

        navigation.WaitForAssertion(() => Assert.Equal(["Все", "Админы"], navigation.FindAll("nav a").Select(link => link.TextContent)));
    }

    [Fact]
    public void Caption_without_visible_sections_disappears()
    {
        AddAuthorization().SetAuthorized("anna");
        new EnfiladeBuilder(Services)
            .AddCaption("Работа").AddSection<EveryonePage>("Все")
            .AddCaption("Доступ").AddSection<AdminsPage>("Админы");

        var navigation = Render<Navigation>();

        navigation.WaitForAssertion(() => Assert.Equal(["Работа"], navigation.FindAll(".nav-caption").Select(caption => caption.TextContent)));
    }

    [Fact]
    public void Badge_shows_the_count()
    {
        AddAuthorization();
        Services.AddSingleton(new Inbox { Count = 3 });
        new EnfiladeBuilder(Services).AddSection<EveryonePage>("Все", section => section.Badge<Inbox>(inbox => inbox.CountAsync()));

        var navigation = Render<Navigation>();

        navigation.WaitForAssertion(() => Assert.Equal("3", navigation.Find("nav a sup.count").TextContent));
        Assert.NotNull(navigation.Find("nav a .sr"));
    }

    [Fact]
    public void Zero_badge_shows_nothing()
    {
        AddAuthorization();
        var inbox = new Inbox();
        Services.AddSingleton(inbox);
        new EnfiladeBuilder(Services).AddSection<EveryonePage>("Все", section => section.Badge<Inbox>(counter => counter.CountAsync()));

        var navigation = Render<Navigation>();

        navigation.WaitForAssertion(() => Assert.Equal(1, inbox.Calls));
        Assert.Empty(navigation.FindAll("sup.count"));
    }

    [Fact]
    public void Failing_badge_leaves_the_rest_alone()
    {
        AddAuthorization();
        Services.AddFakeLogging();
        Services.AddSingleton(new BrokenCounter()).AddSingleton(new Inbox { Count = 4 });
        new EnfiladeBuilder(Services)
            .AddSection<EveryonePage>("Все", section => section.Badge<BrokenCounter>(counter => counter.CountAsync()))
            .AddSection<WelcomePage>("Привет", section => section.Badge<Inbox>(inbox => inbox.CountAsync()));

        var navigation = Render<Navigation>();

        navigation.WaitForAssertion(() => Assert.Equal("4", navigation.Find("nav a sup.count").TextContent));
        Assert.Equal(2, navigation.FindAll("nav a").Count);
        Assert.Single(navigation.FindAll("sup.count"));
        Assert.Contains(Services.GetFakeLogCollector().GetSnapshot(), record => record.Level == LogLevel.Error);
    }

    [Fact]
    public async Task Page_refresh_recounts_badges()
    {
        AddAuthorization();
        var inbox = new Inbox { Count = 2 };
        Services.AddSingleton(inbox);
        new EnfiladeBuilder(Services).AddSection<EveryonePage>("Все", section => section.Badge<Inbox>(counter => counter.CountAsync()));
        var navigation = Render<Navigation>();
        navigation.WaitForAssertion(() => Assert.Equal("2", navigation.Find("sup.count").TextContent));

        inbox.Count = 5;
        await navigation.InvokeAsync(() => Services.GetRequiredService<CurrentPage>().RefreshAsync());

        navigation.WaitForAssertion(() => Assert.Equal("5", navigation.Find("sup.count").TextContent));
    }

    [Fact]
    public void Moving_to_another_page_recounts_badges()
    {
        AddAuthorization();
        var inbox = new Inbox { Count = 1 };
        Services.AddSingleton(inbox);
        new EnfiladeBuilder(Services).AddSection<EveryonePage>("Все", section => section.Badge<Inbox>(counter => counter.CountAsync()));
        var navigation = Render<Navigation>();
        navigation.WaitForAssertion(() => Assert.Equal(1, inbox.Calls));

        Services.GetRequiredService<NavigationManager>().NavigateTo("welcome");

        navigation.WaitForAssertion(() => Assert.Equal(2, inbox.Calls));
    }

    [Fact]
    public void Icon_renders_before_the_title()
    {
        AddAuthorization();
        new EnfiladeBuilder(Services).AddSection<EveryonePage>("Все", section => section.Icon("<path d=\"M0 0h24v24H0z\" />"));

        var navigation = Render<Navigation>();

        var icon = navigation.WaitForElement("nav a svg");
        Assert.Equal("true", icon.GetAttribute("aria-hidden"));
        Assert.NotNull(icon.QuerySelector("path"));
        Assert.Equal("svg", navigation.Find("nav a").FirstElementChild?.LocalName);
    }
}
