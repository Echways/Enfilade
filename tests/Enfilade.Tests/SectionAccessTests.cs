using System.Security.Claims;
using Enfilade.Tests.Hosts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Enfilade.Tests;

public class SectionAccessTests
{
    private static readonly ClaimsPrincipal Anonymous = new(new ClaimsIdentity());

    private static readonly ClaimsPrincipal Member = new(new ClaimsIdentity([new Claim(ClaimTypes.Name, "anna")], "test"));

    private static readonly ClaimsPrincipal Admin = new(new ClaimsIdentity([new Claim(ClaimTypes.Name, "boris"), new Claim(ClaimTypes.Role, "admin")], "test"));

    private readonly IServiceProvider _services = new ServiceCollection()
        .AddLogging()
        .AddAuthorization(authorization => authorization.AddPolicy("admin", policy => policy.RequireRole("admin")))
        .BuildServiceProvider();

    [Fact]
    public async Task Plain_page_is_open() => Assert.True(await AllowsAsync<EveryonePage>(Anonymous));

    [Fact]
    public async Task Role_page_is_hidden_from_others()
    {
        Assert.False(await AllowsAsync<AdminsPage>(Member));
        Assert.True(await AllowsAsync<AdminsPage>(Admin));
    }

    [Fact]
    public async Task Policy_page_follows_the_policy()
    {
        Assert.False(await AllowsAsync<AuditPage>(Member));
        Assert.True(await AllowsAsync<AuditPage>(Admin));
    }

    [Fact]
    public async Task Private_panel_hides_everything_from_anonymous()
    {
        var options = new EnfiladeOptions { LoginPath = "login" };

        Assert.False(await AllowsAsync<EveryonePage>(Anonymous, options));
        Assert.True(await AllowsAsync<WelcomePage>(Anonymous, options));
        Assert.True(await AllowsAsync<EveryonePage>(Member, options));
    }

    private Task<bool> AllowsAsync<TPage>(ClaimsPrincipal user, EnfiladeOptions? options = null) => SectionAccess.AllowsAsync(
        new Section(typeof(TPage), () => typeof(TPage).Name),
        user,
        options ?? new EnfiladeOptions(),
        _services.GetRequiredService<IAuthorizationPolicyProvider>(),
        _services.GetRequiredService<IAuthorizationService>());
}
