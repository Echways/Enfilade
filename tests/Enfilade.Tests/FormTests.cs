using Enfilade.Tests.Hosts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;

namespace Enfilade.Tests;

public class FormTests : PanelContext
{
    private readonly OrderDraft _draft = new();

    private IDialogService Dialogs => Services.GetRequiredService<IDialogService>();

    [Fact]
    public void Invalid_model_is_not_submitted()
    {
        var saved = false;
        var form = Render<DraftForm>(parameters => parameters.Add(host => host.Draft, _draft).Add(host => host.Save, () =>
        {
            saved = true;
            return Task.CompletedTask;
        }));

        form.Find("form").Submit();

        Assert.False(saved);
        Assert.NotEmpty(form.FindAll(".validation-message"));
    }

    [Fact]
    public void Valid_model_is_submitted_once()
    {
        var pending = new TaskCompletionSource();
        var calls = 0;
        _draft.Customer = "Анна";
        var form = Render<DraftForm>(parameters => parameters.Add(host => host.Draft, _draft).Add(host => host.Save, () =>
        {
            calls++;
            return pending.Task;
        }));

        form.Find("form").Submit();
        form.Find("form").Submit();

        Assert.Equal(1, calls);
        Assert.True(form.Find("button[type=submit]").HasAttribute("disabled"));
        pending.SetResult();
        form.WaitForAssertion(() => Assert.False(form.Find("button[type=submit]").HasAttribute("disabled")));
    }

    [Fact]
    public void Field_errors_from_the_server_land_on_the_field()
    {
        _draft.Customer = "Анна";
        var form = Render<DraftForm>(parameters => parameters
            .Add(host => host.Draft, _draft)
            .Add(host => host.Save, () => throw new FieldException(nameof(OrderDraft.Customer), "Уже есть")));

        form.Find("form").Submit();

        var edit = form.FindComponent<EditForm>().Instance.EditContext!;
        Assert.Contains("Уже есть", edit.GetValidationMessages(() => _draft.Customer));
        Assert.Contains("Уже есть", form.Find(".validation-message").TextContent);

        form.Find("input").Change("Борис");
        Assert.Empty(edit.GetValidationMessages(() => _draft.Customer));
    }

    [Fact]
    public void Panel_exception_shows_above_the_button()
    {
        _draft.Customer = "Анна";
        var form = Render<DraftForm>(parameters => parameters
            .Add(host => host.Draft, _draft)
            .Add(host => host.Save, () => throw new PanelException("Лимит исчерпан")));

        form.Find("form").Submit();

        Assert.Equal("Лимит исчерпан", form.Find(".form-error").TextContent);
    }

    [Fact]
    public void Unexpected_failure_shows_the_general_message()
    {
        _draft.Customer = "Анна";
        var form = Render<DraftForm>(parameters => parameters
            .Add(host => host.Draft, _draft)
            .Add(host => host.Save, () => throw new InvalidOperationException("secret detail")));

        form.Find("form").Submit();

        Assert.Equal(EnfiladeText.Failed, form.Find(".form-error").TextContent);
    }

    [Fact]
    public async Task Success_says_saved_refreshes_and_closes_the_sheet()
    {
        var refreshed = false;
        using var source = Services.GetRequiredService<CurrentPage>().Track(() =>
        {
            refreshed = true;
            return Task.CompletedTask;
        });
        var provider = Render<MudDialogProvider>();

        var opening = provider.InvokeAsync(() => Dialogs.ShowFormAsync<DraftSheet, OrderDraft>("Новый заказ"));
        provider.WaitForElement("form input").Change("Анна");
        provider.Find("form").Submit();

        var draft = await opening.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal("Анна", draft?.Customer);
        Assert.Equal(EnfiladeText.Saved, Assert.Single(Snackbar.ShownSnackbars).Message);
        Assert.True(refreshed);
    }

    [Fact]
    public async Task Cancelled_sheet_returns_nothing()
    {
        var provider = Render<MudDialogProvider>();

        var opening = provider.InvokeAsync(() => Dialogs.ShowFormAsync<DraftSheet, OrderDraft>("Новый заказ"));
        provider.WaitForElement(".sheet-head .link").Click();

        Assert.Null(await opening.WaitAsync(TimeSpan.FromSeconds(5)));
    }

    [Fact]
    public void Leaving_a_dirty_form_asks()
    {
        var provider = Render<MudDialogProvider>();
        var form = Render<DraftForm>(parameters => parameters.Add(host => host.Draft, _draft));
        form.Find("input").Change("Анна");

        Services.GetRequiredService<NavigationManager>().NavigateTo("elsewhere");

        provider.WaitForAssertion(() => Assert.Contains(EnfiladeText.DiscardQuestion, provider.Find(".confirm").TextContent));
        Assert.DoesNotContain("elsewhere", Services.GetRequiredService<NavigationManager>().Uri);
    }

    [Fact]
    public void Clean_form_leaves_at_once()
    {
        var provider = Render<MudDialogProvider>();
        Render<DraftForm>(parameters => parameters.Add(host => host.Draft, _draft));

        Services.GetRequiredService<NavigationManager>().NavigateTo("elsewhere");

        Assert.EndsWith("elsewhere", Services.GetRequiredService<NavigationManager>().Uri);
        Assert.Empty(provider.FindAll(".confirm"));
    }
}
