using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Enfilade;

public abstract record NavEntry;

public sealed record Section(Type Page, Func<string> Title) : NavEntry
{
    public string Href { get; } = Route(Page);

    public NavLinkMatch Match => Href.Length == 0 ? NavLinkMatch.All : NavLinkMatch.Prefix;

    private static string Route(Type page) =>
        page.GetCustomAttributes<RouteAttribute>()
            .Select(route => route.Template)
            .FirstOrDefault(template => !template.Contains('{'))
            ?.TrimStart('/')
        ?? throw new InvalidOperationException($"{page.FullName} has no @page route without parameters to open from the navigation");
}

public sealed record Caption(Func<string> Text) : NavEntry;
