using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MudBlazor.Services;
using Enfilade.Layout;

namespace Enfilade;

public static class EnfiladeServices
{
    public static EnfiladeBuilder AddEnfilade(this IServiceCollection services, Action<EnfiladeOptions>? configure = null)
    {
        if (configure is not null)
            services.Configure(configure);

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

    public static IApplicationBuilder UseEnfiladeLanguage(this IApplicationBuilder app) => app.UseRequestLocalization();

    public static RazorComponentsEndpointConventionBuilder MapEnfilade(this IEndpointRouteBuilder endpoints)
    {
        var options = endpoints.ServiceProvider.GetRequiredService<IOptions<EnfiladeOptions>>().Value;
        endpoints.MapStaticAssets();
        endpoints.MapGet("/language/{culture}", LanguageChoice.Choose);
        var panel = endpoints.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
            .AddAdditionalAssemblies(options.AdditionalAssemblies);
        return options.LoginPath is null ? panel : panel.RequireAuthorization();
    }
}
