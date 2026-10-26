using Enfilade.Tests.Hosts;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;

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

    [Fact]
    public async Task Escape_closes_a_clean_sheet()
    {
        var provider = Render<MudDialogProvider>();
        var opening = provider.InvokeAsync(() => Dialogs.ShowFormAsync<DraftSheet, OrderDraft>("Новый заказ"));
        provider.WaitForElement("form input");

        await PressEscapeAsync(provider, ".sheet");

        Assert.Null(await opening.WaitAsync(TimeSpan.FromSeconds(5)));
    }

    [Fact]
    public async Task Escape_on_a_dirty_sheet_asks_before_closing()
    {
        var provider = Render<MudDialogProvider>();
        var opening = provider.InvokeAsync(() => Dialogs.ShowFormAsync<DraftSheet, OrderDraft>("Новый заказ"));
        provider.WaitForElement("form input").Change("Анна");

        var asking = PressEscapeAsync(provider, ".sheet");
        provider.WaitForAssertion(() => Assert.Contains(EnfiladeText.DiscardQuestion, provider.Find(".confirm").TextContent));

        await PressEscapeAsync(provider, ".confirm");
        await asking.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Empty(provider.FindAll(".confirm"));
        Assert.NotEmpty(provider.FindAll(".sheet"));
        Assert.False(opening.IsCompleted);
    }

    private Task PressEscapeAsync(IRenderedComponent<MudDialogProvider> provider, string dialog)
    {
        var keys = Services.GetRequiredService<IKeyInterceptorService>();
        var bubbling = new List<string>();
        for (var element = provider.Find($"[role=dialog]{dialog}"); element is not null; element = element.ParentElement)
        {
            if (!string.IsNullOrEmpty(element.Id))
                bubbling.Add(element.Id);
        }

        return provider.InvokeAsync(async () =>
        {
            foreach (var id in bubbling)
                await keys.DispatchAsync(id, KeyEventKind.Down, new KeyboardEventArgs { Key = "Escape" });
        });
    }
}
