using Enfilade.Tests.Hosts;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;

namespace Enfilade.Tests;

public class SheetTests : PanelContext
{
    private IDialogService Dialogs => Services.GetRequiredService<IDialogService>();

    [Fact]
    public async Task Dirty_sheet_asks_before_closing()
    {
        var provider = Render<MudDialogProvider>();
        var opening = provider.InvokeAsync(() => Dialogs.ShowFormAsync<DraftSheet, OrderDraft>("Новый заказ"));
        provider.WaitForElement("form input").Change("Анна");

        provider.Find(".sheet-head .link").Click();
        provider.WaitForAssertion(() => Assert.Contains(EnfiladeText.DiscardQuestion, provider.Find(".confirm").TextContent));
        provider.Find(".confirm-foot .link").Click();

        provider.WaitForAssertion(() => Assert.Empty(provider.FindAll(".confirm")));
        Assert.NotEmpty(provider.FindAll(".sheet"));
        Assert.False(opening.IsCompleted);

        provider.Find(".sheet-head .link").Click();
        provider.WaitForElement(".confirm-foot .btn-danger").Click();

        Assert.Null(await opening.WaitAsync(TimeSpan.FromSeconds(5)));
    }

    [Fact]
    public async Task Clean_sheet_closes_at_once()
    {
        var provider = Render<MudDialogProvider>();
        var opening = provider.InvokeAsync(() => Dialogs.ShowFormAsync<DraftSheet, OrderDraft>("Новый заказ"));

        provider.WaitForElement(".sheet-head .link").Click();

        Assert.Null(await opening.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.Empty(provider.FindAll(".confirm"));
    }
}
