using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Enfilade.Tests;

public class EnfiladeBuilderTests
{
    [Fact]
    public void Section_opens_the_page_at_its_route()
    {
        var section = Assert.IsType<Section>(Assert.Single(Build(shell => shell.AddSection<Clients>(() => "Clients")).Navigation));

        Assert.Equal("clients", section.Href);
        Assert.Equal(NavLinkMatch.Prefix, section.Match);
        Assert.Equal("Clients", section.Title());
    }

    [Fact]
    public void Section_at_the_root_is_current_only_on_the_root()
    {
        var section = Assert.IsType<Section>(Assert.Single(Build(shell => shell.AddSection<Home>(() => "Home")).Navigation));

        Assert.Equal("", section.Href);
        Assert.Equal(NavLinkMatch.All, section.Match);
    }

    [Fact]
    public void Section_takes_the_route_without_parameters()
    {
        var section = Assert.IsType<Section>(Assert.Single(Build(shell => shell.AddSection<Client>(() => "Client")).Navigation));

        Assert.Equal("client", section.Href);
    }

    [Fact]
    public void Page_reachable_only_with_parameters_cannot_be_a_section()
    {
        var error = Assert.Throws<InvalidOperationException>(() => new ServiceCollection().AddEnfilade().AddSection<Device>(() => "Device"));

        Assert.Contains(nameof(Device), error.Message);
    }

    [Fact]
    public void Navigation_keeps_the_order_of_registration()
    {
        var options = Build(shell => shell
            .AddSection<Home>(() => "Home")
            .AddCaption(() => "Access")
            .AddSection<Clients>(() => "Clients"));

        Assert.Collection(
            options.Navigation,
            entry => Assert.Equal("Home", Assert.IsType<Section>(entry).Title()),
            entry => Assert.Equal("Access", Assert.IsType<Caption>(entry).Text()),
            entry => Assert.Equal("Clients", Assert.IsType<Section>(entry).Title()));
    }

    [Fact]
    public void Section_brings_the_pages_of_its_assembly()
    {
        var options = Build(shell => shell.AddSection<Clients>(() => "Clients").AddSection<Home>(() => "Home"));

        Assert.Equal([typeof(Clients).Assembly], options.Assemblies);
    }

    [Fact]
    public void Plain_titles_need_no_lambda()
    {
        var options = Build(shell => shell.AddCaption("Access").AddSection<Clients>("Clients"));

        Assert.Equal("Access", Assert.IsType<Caption>(options.Navigation[0]).Text());
        Assert.Equal("Clients", Assert.IsType<Section>(options.Navigation[1]).Title());
    }

    [Fact]
    public void Page_finds_its_own_section()
    {
        var options = Build(shell => shell.AddSection<Home>("Home").AddSection<Clients>("Clients"));

        Assert.Equal("Clients", options.SectionOf(typeof(Clients))?.Title());
        Assert.Null(options.SectionOf(typeof(Client)));
    }

    [Fact]
    public void Without_a_login_every_page_is_open() =>
        Assert.False(new EnfiladeOptions().Guards(typeof(Clients)));

    [Fact]
    public void With_a_login_pages_are_private_without_saying_so() =>
        Assert.True(new EnfiladeOptions { LoginPath = "login" }.Guards(typeof(Clients)));

    [Fact]
    public void With_a_login_anonymous_pages_stay_open()
    {
        var options = new EnfiladeOptions { LoginPath = "login" };

        Assert.False(options.Guards(typeof(Welcome)));
        Assert.False(options.Guards(typeof(Signup)));
    }

    [Fact]
    public void Stylesheets_follow_the_order_of_registration()
    {
        var options = Build(shell => shell.AddStylesheet("a.css").AddStylesheet("b.css"));

        Assert.Equal(["a.css", "b.css"], options.Stylesheets);
    }

    private static EnfiladeOptions Build(Action<EnfiladeBuilder> configure)
    {
        var services = new ServiceCollection();
        configure(services.AddEnfilade());
        return services.BuildServiceProvider().GetRequiredService<IOptions<EnfiladeOptions>>().Value;
    }

    [Route("/")]
    private sealed class Home : ComponentBase;

    [Route("/clients")]
    private sealed class Clients : ComponentBase;

    [Route("/client/{id:guid}")]
    [Route("/client")]
    private sealed class Client : ComponentBase;

    [Route("/devices/{id}")]
    private sealed class Device : ComponentBase;

    [AllowAnonymous]
    [Route("/welcome")]
    private class Welcome : ComponentBase;

    [Route("/signup")]
    private sealed class Signup : Welcome;
}
