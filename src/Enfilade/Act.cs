using Microsoft.Extensions.Logging;
using MudBlazor;

namespace Enfilade;

public sealed record Confirm(string Question, string Message, string Yes, bool Danger = false);

public sealed class PanelException(string message) : Exception(message);

public sealed class Act(IDialogService dialogs, ISnackbar snackbar, CurrentPage page, ILogger<Act> logger)
{
    public async Task<bool> RunAsync(Func<Task> action, string? done = null, Confirm? confirm = null, bool refresh = true)
    {
        if (confirm is not null && !await dialogs.ConfirmAsync(confirm.Question, confirm.Message, confirm.Yes, confirm.Danger))
            return false;

        try
        {
            await action();
        }
        catch (PanelException exception)
        {
            snackbar.Add(exception.Message, Severity.Error);
            return false;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Enfilade: an action failed");
            snackbar.Add(EnfiladeText.Failed, Severity.Error);
            return false;
        }

        if (done is not null)
            snackbar.Add(done, Severity.Normal);
        if (refresh)
            await page.RefreshAsync();
        return true;
    }
}
