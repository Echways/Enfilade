using Enfilade.Tests.Hosts;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Enfilade.Tests;

public class MenuTests : PanelContext
{
    private IRenderedComponent<MudPopoverProvider> Popovers => field ??= Render<MudPopoverProvider>();

    private IRenderedComponent<RowMenu> Open(Action<ComponentParameterCollectionBuilder<RowMenu>>? parameters = null)
    {
        var popovers = Popovers;
        var row = Render(parameters ?? (_ => { }));
        row.Find("button").Click();
        popovers.WaitForAssertion(() => Assert.Contains("Удалить", popovers.Markup));
        return row;
    }

    private static RenderFragment Nothing => builder => builder.AddContent(0, "");

    [Fact]
    public void Trigger_is_named_for_screen_readers()
    {
        var menu = Render<Menu>(parameters => parameters.Add(menu => menu.ChildContent, Nothing));

        Assert.Equal(EnfiladeText.RowActions, menu.Find("button").GetAttribute("aria-label"));
    }

    [Fact]
    public void Label_names_the_trigger()
    {
        var menu = Render<Menu>(parameters => parameters
            .Add(menu => menu.Label, "Действия с заказом")
            .Add(menu => menu.ChildContent, Nothing));

        Assert.Equal("Действия с заказом", menu.Find("button").GetAttribute("aria-label"));
    }

    [Fact]
    public void Opening_the_menu_does_not_click_the_row()
    {
        var row = Open();

        Assert.Equal(0, row.Instance.Opened);
    }

    [Fact]
    public void Danger_item_is_marked()
    {
        Open(parameters => parameters.Add(row => row.Danger, true));

        Assert.Single(Popovers.FindAll(".mud-menu-item.danger"));
    }

    [Fact]
    public void Item_runs_the_action_through_act_and_says_done()
    {
        var clicks = 0;
        Open(parameters => parameters
            .Add(row => row.OnClick, () => clicks++)
            .Add(row => row.Done, "Удалено"));

        Popovers.Find(".mud-menu-item").Click();

        Popovers.WaitForAssertion(() => Assert.Equal("Удалено", Assert.Single(Snackbar.ShownSnackbars).Message));
        Assert.Equal(1, clicks);
    }

    [Fact]
    public void Failing_item_shows_the_failure_instead_of_throwing()
    {
        Open(parameters => parameters.Add(row => row.OnClick, () => throw new PanelException("Заказ закрыт")));

        Popovers.Find(".mud-menu-item").Click();

        Popovers.WaitForAssertion(() => Assert.Equal("Заказ закрыт", Assert.Single(Snackbar.ShownSnackbars).Message));
    }
}
