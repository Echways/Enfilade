using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MudBlazor.Services;
using Enfilade.Layout;

namespace Enfilade;

public static class EnfiladeServices
{
    public const string ConfigurationSection = "Enfilade";

    public static EnfiladeBuilder AddEnfilade(this IServiceCollection services, Action<EnfiladeOptions>? configure = null)
    {
        var options = services.AddOptions<EnfiladeOptions>()
            .BindConfiguration(ConfigurationSection)
            .Validate(EnfiladeOptions.SpeaksItsLanguage, "Enfilade: Language must be one of Languages")
            .Validate(EnfiladeOptions.RefreshesAtAll, "Enfilade: AutoRefreshInterval must be positive")
            .ValidateOnStart();
        if (configure is not null)
            options.Configure(configure);

        services.AddAuthorizationCore();
        services.AddCascadingAuthenticationState();
        services.AddRazorComponents().AddInteractiveServerComponents();
        services.AddMudServices(mud =>
        {
            mud.SnackbarConfiguration.PositionClass = MudBlazor.Defaults.Classes.Position.BottomLeft;
            mud.SnackbarConfiguration.HideIcon = true;
            mud.SnackbarConfiguration.ShowCloseIcon = false;
            mud.SnackbarConfiguration.ShowTransitionDuration = 160;
            mud.SnackbarConfiguration.HideTransitionDuration = 160;
        });
        services.AddScoped<Clipboard>();
        services.AddOptions<RequestLocalizationOptions>().Configure<IOptions<EnfiladeOptions>>((localization, shell) =>
        {
            var spoken = LanguageChoice.Spoken(shell.Value).ToArray();
            localization.SetDefaultCulture(shell.Value.Language)
                .AddSupportedCultures(spoken)
                .AddSupportedUICultures(spoken);
            localization.RequestCultureProviders = [new CookieRequestCultureProvider()];
        });
        return new EnfiladeBuilder(services);
    }

    public static RazorComponentsEndpointConventionBuilder MapEnfilade(this WebApplication app)
    {
        var options = app.Services.GetRequiredService<IOptions<EnfiladeOptions>>().Value;
        app.UseRequestLocalization();
        app.UseAntiforgery();
        app.MapStaticAssets();
        app.MapGet("/language/{culture}", LanguageChoice.Choose);
        var panel = app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
            .AddAdditionalAssemblies(options.AdditionalAssemblies);
        return options.LoginPath is null ? panel : panel.RequireAuthorization();
    }
}
