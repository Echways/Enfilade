using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;

namespace Enfilade;

internal static class LanguageChoice
{
    private static readonly TimeSpan Remembered = TimeSpan.FromDays(365);

    public static IReadOnlyList<string> Spoken(EnfiladeOptions options) =>
        options.Languages.Count == 0 ? [options.Language] : [.. options.Languages];

    public static CultureInfo? Next(EnfiladeOptions options)
    {
        var spoken = Spoken(options);
        if (spoken.Count < 2)
            return null;

        var current = spoken.ToList().IndexOf(CultureInfo.CurrentUICulture.Name);
        return CultureInfo.GetCultureInfo(spoken[(current + 1) % spoken.Count]);
    }

    public static string Title(CultureInfo culture) => Words.Sentence(culture.NativeName);

    public static string Path(NavigationManager navigation, CultureInfo culture) =>
        $"language/{culture.Name}?back={Uri.EscapeDataString(navigation.ToBaseRelativePath(navigation.Uri))}";

    internal static IResult Choose(HttpContext context, string culture, string? back, IOptions<EnfiladeOptions> options)
    {
        if (!Spoken(options.Value).Contains(culture, StringComparer.Ordinal))
            return Results.NotFound();

        var home = context.Request.PathBase + "/";
        context.Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions { Path = home, MaxAge = Remembered, HttpOnly = true, IsEssential = true, SameSite = SameSiteMode.Lax });
        return Results.LocalRedirect(home + back?.TrimStart('/', '\\'));
    }
}
