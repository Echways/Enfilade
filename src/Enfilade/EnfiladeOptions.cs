using System.Reflection;
using Microsoft.AspNetCore.Authorization;

namespace Enfilade;

public sealed class EnfiladeOptions
{
    public string Brand { get; set; } = "Enfilade";

    public string? Favicon { get; set; }

    public string Language { get; set; } = "en";

    public IList<string> Languages { get; set; } = [];

    public string? LoginPath { get; set; }

    public string? SignOutPath { get; set; }

    public string? TimeZone { get; set; }

    public TimeSpan AutoRefreshInterval { get; set; } = TimeSpan.FromSeconds(30);

    public IList<string> Stylesheets { get; } = [];

    public IList<NavEntry> Navigation { get; } = [];

    public ISet<Assembly> Assemblies { get; } = new HashSet<Assembly>();

    internal static bool SpeaksItsLanguage(EnfiladeOptions options) =>
        options.Languages.Count == 0 || options.Languages.Contains(options.Language, StringComparer.Ordinal);

    internal static bool KnowsItsTimeZone(EnfiladeOptions options) =>
        options.TimeZone is null || TimeZoneInfo.TryFindSystemTimeZoneById(options.TimeZone, out _);

    internal static bool RefreshesAtAll(EnfiladeOptions options) => options.AutoRefreshInterval > TimeSpan.Zero;

    internal Assembly[] AdditionalAssemblies => [.. Assemblies.Where(assembly => assembly != typeof(EnfiladeOptions).Assembly)];

    internal bool Guards(Type page) =>
        LoginPath is not null && !page.GetCustomAttributes(inherit: true).OfType<IAllowAnonymous>().Any();

    internal Section? SectionOf(Type page) => Navigation.OfType<Section>().FirstOrDefault(section => section.Page == page);
}
