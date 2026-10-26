using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;

namespace Enfilade.Tests;

public abstract class PanelContext : BunitContext
{
    protected PanelContext()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        var shell = new ServiceCollection();
        shell.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        shell.AddEnfilade();
        shell.AddLogging();
        var taken = Services.Select(service => service.ServiceType).ToHashSet();
        foreach (var service in shell.Where(service => !taken.Contains(service.ServiceType)))
            Services.Add(service);
    }

    protected Act Act => Services.GetRequiredService<Act>();

    protected ISnackbar Snackbar => Services.GetRequiredService<ISnackbar>();
}
