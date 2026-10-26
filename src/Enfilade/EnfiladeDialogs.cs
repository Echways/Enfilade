using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Enfilade;

public static class EnfiladeDialogs
{
    public static async Task<bool> ConfirmAsync(this IDialogService dialogs, string question, string message, string yes, bool danger = false)
    {
        var parameters = new DialogParameters<ConfirmDialog>
        {
            { dialog => dialog.Message, message },
            { dialog => dialog.Yes, yes },
            { dialog => dialog.Danger, danger },
        };
        var dialog = await dialogs.ShowAsync<ConfirmDialog>(question, parameters, ConfirmDialog.Centered);
        return await dialog.Result is { Canceled: false };
    }

    public static Task<IDialogReference> ShowSheetAsync<TSheet>(
        this IDialogService dialogs, string title, DialogParameters<TSheet>? parameters = null, bool form = true)
        where TSheet : IComponent =>
        dialogs.ShowAsync<TSheet>(title, parameters ?? new DialogParameters<TSheet>(), form ? Sheet.Form : Sheet.View);
}
