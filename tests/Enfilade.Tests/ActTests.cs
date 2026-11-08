using Enfilade.Tests.Hosts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using MudBlazor;

namespace Enfilade.Tests;

public class ActTests : PanelContext
{
    [Fact]
    public async Task Success_says_done_and_refreshes_the_page()
    {
        var refreshed = false;
        using var source = Services.GetRequiredService<CurrentPage>().Track(() => { refreshed = true; return Task.CompletedTask; });

        Assert.Equal(ActResult.Done, await Act.RunAsync(() => Task.CompletedTask, done: "Готово"));
        Assert.Equal("Готово", Assert.Single(Snackbar.ShownSnackbars).Message);
        Assert.True(refreshed);
    }

    [Fact]
    public async Task Panel_exception_is_shown_as_is()
    {
        Assert.Equal(ActResult.Failed, await Act.RunAsync(() => throw new PanelException("Лимит исчерпан")));
        Assert.Equal("Лимит исчерпан", Assert.Single(Snackbar.ShownSnackbars).Message);
    }

    [Fact]
    public async Task Registered_exception_shows_its_message()
    {
        Services.ShowMessageOf<Refusal>(refusal => $"Отказ: {refusal.Message}");

        Assert.Equal(ActResult.Failed, await Act.RunAsync(() => throw new QuotaRefusal("квота")));
        Assert.Equal("Отказ: квота", Assert.Single(Snackbar.ShownSnackbars).Message);
    }

    [Fact]
    public async Task Other_exceptions_show_a_general_message()
    {
        Assert.Equal(ActResult.Failed, await Act.RunAsync(() => throw new InvalidOperationException("secret detail")));
        Assert.Equal(EnfiladeText.Failed, Assert.Single(Snackbar.ShownSnackbars).Message);
    }

    [Fact]
    public async Task Timeout_is_a_failure_that_is_shown_and_logged()
    {
        Services.AddFakeLogging();

        Assert.Equal(ActResult.Failed, await Act.RunAsync(() => throw new TaskCanceledException("timed out", new TimeoutException())));
        Assert.Equal(EnfiladeText.Failed, Assert.Single(Snackbar.ShownSnackbars).Message);
        Assert.Contains(Services.GetFakeLogCollector().GetSnapshot(), record => record.Level == LogLevel.Error);
    }

    [Fact]
    public async Task Cancelled_action_ends_quietly()
    {
        Assert.Equal(ActResult.Cancelled, await Act.RunAsync(() => throw new OperationCanceledException()));
        Assert.Empty(Snackbar.ShownSnackbars);
    }

    [Fact]
    public async Task Declined_confirmation_runs_nothing()
    {
        var provider = Render<MudDialogProvider>();
        var ran = false;
        var running = Act.RunAsync(() => { ran = true; return Task.CompletedTask; }, confirm: new Confirm("Удалить?", "Навсегда.", "Удалить", Danger: true));
        provider.Find(".confirm-foot .link").Click();

        Assert.Equal(ActResult.Declined, await running);
        Assert.False(ran);
    }

    [Fact]
    public async Task Failure_refreshes_the_page_so_it_shows_the_real_state()
    {
        var refreshed = false;
        using var source = Services.GetRequiredService<CurrentPage>().Track(() => { refreshed = true; return Task.CompletedTask; });

        Assert.Equal(ActResult.Failed, await Act.RunAsync(() => throw new PanelException("Клиент не найден")));
        Assert.True(refreshed);
    }

    [Fact]
    public async Task Failure_does_not_refresh_when_refresh_is_off()
    {
        var refreshed = false;
        using var source = Services.GetRequiredService<CurrentPage>().Track(() => { refreshed = true; return Task.CompletedTask; });

        Assert.Equal(ActResult.Failed, await Act.RunAsync(() => throw new PanelException("Клиент не найден"), refresh: false));
        Assert.False(refreshed);
    }

    [Fact]
    public async Task Cancelled_action_and_declined_confirmation_leave_the_page_alone()
    {
        var refreshed = false;
        using var source = Services.GetRequiredService<CurrentPage>().Track(() => { refreshed = true; return Task.CompletedTask; });
        var provider = Render<MudDialogProvider>();

        Assert.Equal(ActResult.Cancelled, await Act.RunAsync(() => throw new OperationCanceledException()));
        var running = Act.RunAsync(() => Task.CompletedTask, confirm: new Confirm("Удалить?", "Навсегда.", "Удалить"));
        provider.Find(".confirm-foot .link").Click();

        Assert.Equal(ActResult.Declined, await running);
        Assert.False(refreshed);
    }

    [Fact]
    public async Task Confirmed_action_runs_and_is_done()
    {
        var provider = Render<MudDialogProvider>();
        var ran = false;
        var running = Act.RunAsync(() => { ran = true; return Task.CompletedTask; }, confirm: new Confirm("Удалить?", "Навсегда.", "Удалить"));
        provider.Find(".confirm-foot .btn-k, .confirm-foot .btn-danger").Click();

        Assert.Equal(ActResult.Done, await running);
        Assert.True(ran);
    }
}
