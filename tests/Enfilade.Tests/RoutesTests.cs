using Enfilade.Layout;
using Enfilade.Tests.Hosts;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Enfilade.Tests;

public class RoutesTests : PanelContext
{
    private NavigationManager Navigation => Services.GetRequiredService<NavigationManager>();

    [Fact]
    public void Signed_in_user_without_rights_sees_no_access()
    {
        AddAuthorization().SetAuthorized("anna");
        new EnfiladeBuilder(Services).AddSection<EveryonePage>("Все").AddSection<AdminsPage>("Админы");
        Navigation.NavigateTo("admins");

        var routes = Render<Routes>();

        routes.WaitForAssertion(() => Assert.Contains(EnfiladeText.ForbiddenHeading, routes.Find("main").TextContent));
        Assert.NotEmpty(routes.FindAll("nav a"));
    }

    [Fact]
    public void Anonymous_visitor_goes_to_sign_in()
    {
        AddAuthorization();
        new EnfiladeBuilder(Services).AddSection<AdminsPage>("Админы");
        Services.Configure<EnfiladeOptions>(options => options.LoginPath = "login");
        Navigation.NavigateTo("admins");

        Render<Routes>();

        Assert.EndsWith("/login", Navigation.Uri);
    }
}
