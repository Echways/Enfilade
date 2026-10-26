using Microsoft.Extensions.DependencyInjection;
using MudBlazor;

namespace Enfilade.Tests;

public class ActTests : PanelContext
{
    [Fact]
    public async Task Success_says_done_and_refreshes_the_page()
    {
        var refreshed = false;
        using var source = Services.GetRequiredService<CurrentPage>().Track(() => { refreshed = true; return Task.CompletedTask; });

        Assert.True(await Act.RunAsync(() => Task.CompletedTask, done: "Готово"));
        Assert.Equal("Готово", Assert.Single(Snackbar.ShownSnackbars).Message);
        Assert.True(refreshed);
    }

    [Fact]
    public async Task Panel_exception_is_shown_as_is()
    {
        Assert.False(await Act.RunAsync(() => throw new PanelException("Лимит исчерпан")));
        Assert.Equal("Лимит исчерпан", Assert.Single(Snackbar.ShownSnackbars).Message);
    }

    [Fact]
    public async Task Other_exceptions_show_a_general_message()
    {
        Assert.False(await Act.RunAsync(() => throw new InvalidOperationException("secret detail")));
        Assert.Equal(EnfiladeText.Failed, Assert.Single(Snackbar.ShownSnackbars).Message);
    }

    [Fact]
    public async Task Declined_confirmation_runs_nothing()
    {
        var provider = Render<MudDialogProvider>();
        var ran = false;
        var running = Act.RunAsync(() => { ran = true; return Task.CompletedTask; }, confirm: new Confirm("Удалить?", "Навсегда.", "Удалить", Danger: true));
        provider.Find(".confirm-foot .link").Click();

        Assert.False(await running);
        Assert.False(ran);
    }
}
