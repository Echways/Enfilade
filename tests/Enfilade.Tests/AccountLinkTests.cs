using Enfilade.Layout;

namespace Enfilade.Tests;

public class AccountLinkTests : PanelContext
{
    [Fact]
    public void Account_links_appear_in_both_account_views()
    {
        AddAuthorization().SetAuthorized("anna");
        new EnfiladeBuilder(Services).AddAccountLink("Профиль", "profile");

        var layout = Render<MainLayout>(parameters => parameters.Add(layout => layout.Body, "<p>Страница</p>"));

        layout.WaitForAssertion(() => Assert.Equal("profile", layout.Find(".account a").GetAttribute("href")));
        layout.Find(".account-menu button").Click();
        layout.WaitForAssertion(() => Assert.Contains(layout.FindAll(".mud-menu-item"), item => item.TextContent.Contains("Профиль")));
    }
}
