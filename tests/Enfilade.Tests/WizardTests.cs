using Enfilade.Tests.Hosts;
using Microsoft.Extensions.DependencyInjection;

namespace Enfilade.Tests;

public class WizardTests : PanelContext
{
    private IRenderedComponent<OrderWizard> Open(Action<ComponentParameterCollectionBuilder<OrderWizard>>? parameters = null) =>
        Render(parameters ?? (_ => { }));

    private static string[] Titles(IRenderedComponent<OrderWizard> wizard) =>
        [.. wizard.FindAll(".step-title").Select(title => title.TextContent)];

    private static string[] States(IRenderedComponent<OrderWizard> wizard) =>
        [.. wizard.FindAll(".step").Select(step => step.ClassList.Last())];

    [Fact]
    public void Header_lists_every_step_on_the_first_render()
    {
        var wizard = Open(parameters => parameters.Add(host => host.Detail, "Анна"));

        Assert.Equal(["Клиент", "Доставка", "Оплата"], Titles(wizard));
        Assert.Equal("Анна", Assert.Single(wizard.FindAll(".step-detail")).TextContent);
        Assert.Equal(["current", "pending", "pending"], States(wizard));
    }

    [Fact]
    public void Only_the_current_step_is_shown()
    {
        var wizard = Open();

        Assert.Single(wizard.FindAll(".customer"));
        Assert.Empty(wizard.FindAll(".delivery"));
        Assert.Empty(wizard.FindAll(".payment"));
    }

    [Fact]
    public void Next_moves_on_and_marks_the_passed_step_done()
    {
        var wizard = Open();

        wizard.Find(".btn-k").Click();

        wizard.WaitForAssertion(() => Assert.Single(wizard.FindAll(".delivery")));
        Assert.Empty(wizard.FindAll(".customer"));
        Assert.Equal(["done", "current", "pending"], States(wizard));
    }

    [Fact]
    public void Back_is_offered_after_the_first_step_and_goes_back()
    {
        var wizard = Open();
        Assert.Empty(wizard.FindAll("button.link"));

        wizard.Find(".btn-k").Click();
        wizard.WaitForAssertion(() => Assert.Single(wizard.FindAll(".delivery")));
        wizard.Find("button.link").Click();

        Assert.Single(wizard.FindAll(".customer"));
        Assert.Equal(["current", "pending", "pending"], States(wizard));
    }

    [Fact]
    public void Current_can_be_bound_and_set_from_outside()
    {
        var wizard = Open(parameters => parameters.Add(host => host.Current, 1));

        Assert.Single(wizard.FindAll(".delivery"));
        Assert.Equal(["done", "current", "pending"], States(wizard));
    }

    [Fact]
    public void Current_beyond_the_last_step_stops_at_the_last_one()
    {
        var wizard = Open(parameters => parameters.Add(host => host.Current, 9));

        Assert.Single(wizard.FindAll(".payment"));
    }

    [Fact]
    public void Last_step_offers_finish_instead_of_next()
    {
        var wizard = Open(parameters => parameters.Add(host => host.Current, 2));

        Assert.Equal(EnfiladeText.Finish, wizard.Find(".btn-k").TextContent);
    }

    [Fact]
    public void Finish_runs_through_act_and_says_done()
    {
        var finished = 0;
        var wizard = Open(parameters => parameters
            .Add(host => host.Current, 2)
            .Add(host => host.OnFinish, () => finished++)
            .Add(host => host.Done, "Заказ создан"));

        wizard.Find(".btn-k").Click();

        wizard.WaitForAssertion(() => Assert.Equal("Заказ создан", Assert.Single(Snackbar.ShownSnackbars).Message));
        Assert.Equal(1, finished);
    }

    [Fact]
    public void Leaving_a_step_runs_its_check_first()
    {
        var order = new List<string>();
        var wizard = Open(parameters => parameters.Add(host => host.OnLeave, () => order.Add("leave")));

        wizard.Find(".btn-k").Click();

        wizard.WaitForAssertion(() => Assert.Single(wizard.FindAll(".delivery")));
        Assert.Equal(["leave"], order);
    }

    [Fact]
    public void Refused_step_stays_marked_failed_and_the_reason_is_shown()
    {
        var wizard = Open(parameters => parameters.Add(host => host.OnLeave, () => throw new PanelException("Укажите клиента")));

        wizard.Find(".btn-k").Click();

        wizard.WaitForAssertion(() => Assert.Equal("Укажите клиента", Assert.Single(Snackbar.ShownSnackbars).Message));
        Assert.Single(wizard.FindAll(".customer"));
        Assert.Equal(["failed", "pending", "pending"], States(wizard));
    }

    [Fact]
    public void Refusal_is_forgotten_once_the_step_is_left()
    {
        var refuse = true;
        var wizard = Open(parameters => parameters.Add(host => host.OnLeave, () =>
        {
            if (refuse)
                throw new PanelException("Укажите клиента");
        }));
        wizard.Find(".btn-k").Click();
        wizard.WaitForAssertion(() => Assert.Equal(["failed", "pending", "pending"], States(wizard)));

        refuse = false;
        wizard.Find(".btn-k").Click();

        wizard.WaitForAssertion(() => Assert.Equal(["done", "current", "pending"], States(wizard)));
    }

    [Fact]
    public void Refused_finish_does_not_finish_or_say_done()
    {
        var finished = 0;
        var wizard = Open(parameters => parameters
            .Add(host => host.Current, 2)
            .Add(host => host.OnLeavePayment, () => throw new PanelException("Выберите способ оплаты"))
            .Add(host => host.OnFinish, () => finished++)
            .Add(host => host.Done, "Заказ создан"));

        wizard.Find(".btn-k").Click();

        wizard.WaitForAssertion(() => Assert.Equal("Выберите способ оплаты", Assert.Single(Snackbar.ShownSnackbars).Message));
        Assert.Equal(0, finished);
        Assert.Equal(["done", "done", "failed"], States(wizard));
    }

    [Fact]
    public void Moving_between_steps_does_not_reload_the_page()
    {
        var refreshed = 0;
        using var source = Services.GetRequiredService<CurrentPage>().Track(() => { refreshed++; return Task.CompletedTask; });
        var wizard = Open();

        wizard.Find(".btn-k").Click();

        wizard.WaitForAssertion(() => Assert.Single(wizard.FindAll(".delivery")));
        Assert.Equal(0, refreshed);
    }

    [Fact]
    public void Finishing_reloads_the_page_unless_told_not_to()
    {
        var refreshed = 0;
        using var source = Services.GetRequiredService<CurrentPage>().Track(() => { refreshed++; return Task.CompletedTask; });
        var wizard = Open(parameters => parameters.Add(host => host.Current, 2));

        wizard.Find(".btn-k").Click();

        wizard.WaitForAssertion(() => Assert.Equal(1, refreshed));
    }

    [Fact]
    public void A_step_that_appears_later_joins_the_header()
    {
        var wizard = Open(parameters => parameters.Add(host => host.Payment, false));
        Assert.Equal(["Клиент", "Доставка"], Titles(wizard));

        wizard.Render(parameters => parameters.Add(host => host.Payment, true));

        Assert.Equal(["Клиент", "Доставка", "Оплата"], Titles(wizard));
    }

    [Fact]
    public void A_step_that_goes_away_leaves_the_header()
    {
        var wizard = Open();

        wizard.Render(parameters => parameters.Add(host => host.Payment, false));

        Assert.Equal(["Клиент", "Доставка"], Titles(wizard));
    }

    [Fact]
    public void Removing_the_whole_wizard_is_quiet()
    {
        var wizard = Open();

        wizard.Render(parameters => parameters.Add(host => host.Hidden, true));

        Assert.Empty(wizard.FindAll(".wizard"));
    }
}
