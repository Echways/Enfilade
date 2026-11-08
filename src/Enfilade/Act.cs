using Microsoft.Extensions.Logging;
using MudBlazor;

namespace Enfilade;

public sealed record Confirm(string Question, string Message, string Yes, bool Danger = false);

public sealed class Act(IDialogService dialogs, ISnackbar snackbar, CurrentPage page, ErrorMessages errors, ILogger<Act> logger)
{
    public async Task<ActResult> RunAsync(Func<Task> action, string? done = null, Confirm? confirm = null, bool refresh = true)
    {
        if (confirm is not null && !await dialogs.ConfirmAsync(confirm.Question, confirm.Message, confirm.Yes, confirm.Danger))
            return ActResult.Declined;

        var result = await ExecuteAsync(action, done);
        if (refresh && result is ActResult.Done or ActResult.Failed)
            await page.RefreshAsync();
        return result;
    }

    private async Task<ActResult> ExecuteAsync(Func<Task> action, string? done)
    {
        try
        {
            await action();
        }
        catch (Exception exception) when (ErrorMessages.Cancelled(exception))
        {
            return ActResult.Cancelled;
        }
        catch (Exception exception)
        {
            snackbar.Add(errors.Describe(exception, logger, EnfiladeText.Failed), Severity.Error);
            return ActResult.Failed;
        }

        if (done is not null)
            snackbar.Add(done, Severity.Normal);
        return ActResult.Done;
    }
}
