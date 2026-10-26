using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Enfilade.Tests;

public class EnfiladeOptionsTests
{
    [Fact]
    public void Options_come_from_the_enfilade_section()
    {
        var options = Resolve(new Dictionary<string, string?>
        {
            ["Enfilade:Brand"] = "orders",
            ["Enfilade:LoginPath"] = "login",
            ["Enfilade:Languages:0"] = "en",
            ["Enfilade:Languages:1"] = "ru",
            ["Enfilade:AutoRefreshInterval"] = "00:01:00",
        });

        Assert.Equal("orders", options.Brand);
        Assert.Equal("login", options.LoginPath);
        Assert.Equal(["en", "ru"], options.Languages);
        Assert.Equal(TimeSpan.FromMinutes(1), options.AutoRefreshInterval);
    }

    [Fact]
    public void Code_overrides_configuration()
    {
        var options = Resolve(new Dictionary<string, string?> { ["Enfilade:Brand"] = "orders" }, shell => shell.Brand = "billing");

        Assert.Equal("billing", options.Brand);
    }

    [Fact]
    public void Default_language_must_be_offered()
    {
        var error = Assert.Throws<OptionsValidationException>(() => Resolve([], shell =>
        {
            shell.Language = "de";
            shell.Languages = ["en", "ru"];
        }));

        Assert.Contains("Language", error.Message);
    }

    [Fact]
    public void Auto_refresh_must_tick()
    {
        var error = Assert.Throws<OptionsValidationException>(() => Resolve([], shell => shell.AutoRefreshInterval = TimeSpan.Zero));

        Assert.Contains(nameof(EnfiladeOptions.AutoRefreshInterval), error.Message);
    }

    private static EnfiladeOptions Resolve(Dictionary<string, string?> settings, Action<EnfiladeOptions>? configure = null)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(settings).Build());
        services.AddEnfilade(configure);
        return services.BuildServiceProvider().GetRequiredService<IOptions<EnfiladeOptions>>().Value;
    }
}
