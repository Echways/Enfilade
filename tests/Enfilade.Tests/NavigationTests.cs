using Enfilade.Layout;
using Enfilade.Tests.Hosts;

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
}
