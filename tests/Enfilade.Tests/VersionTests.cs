using System.Reflection;
using Enfilade.Layout;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Enfilade.Tests;

public class VersionTests : PanelContext
{
    [Theory]
    [InlineData(null)]
    [InlineData("1.2.3-rc.1")]
    public void The_layout_shows_the_configured_version_or_the_library_version(string? version)
    {
        ConfigureVersion(version, showVersion: true);
        var expected = version ?? typeof(MainLayout).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion.Split('+')[0];

        var layout = Render<MainLayout>();

        Assert.EndsWith(expected, layout.Find(".version").TextContent, StringComparison.Ordinal);
    }

    [Fact]
    public void The_layout_hides_a_configured_version_when_disabled()
    {
        ConfigureVersion("1.2.3", showVersion: false);

        var layout = Render<MainLayout>();

        Assert.Empty(layout.FindAll(".version"));
    }

    private void ConfigureVersion(string? version, bool showVersion)
    {
        AddAuthorization();
        var settings = new Dictionary<string, string?> { ["Enfilade:ShowVersion"] = showVersion.ToString() };
        if (version is not null)
            settings["Enfilade:Version"] = version;
        Services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(settings).Build());
    }
}
