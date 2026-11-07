using Enfilade.Tests.Hosts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Enfilade.Tests;

public class ErrorMessagesTests
{
    [Fact]
    public void Panel_exception_speaks_for_itself()
    {
        Assert.Equal("Лимит исчерпан", Build(_ => { }).Of(new PanelException("Лимит исчерпан")));
    }

    [Fact]
    public void Registered_exception_and_its_subclasses_are_shown()
    {
        var errors = Build(shell => shell.ShowMessageOf<Refusal>(refusal => $"Отказ: {refusal.Message}"));

        Assert.Equal("Отказ: мест нет", errors.Of(new Refusal("мест нет")));
        Assert.Equal("Отказ: квота", errors.Of(new QuotaRefusal("квота")));
    }

    [Fact]
    public void Unregistered_exception_has_no_message()
    {
        var errors = Build(shell => shell.ShowMessageOf<Refusal>(refusal => refusal.Message));

        Assert.Null(errors.Of(new InvalidOperationException("secret detail")));
    }

    [Fact]
    public void Declined_message_passes_to_the_next_registration()
    {
        var errors = Build(shell => shell
            .ShowMessageOf<QuotaRefusal>(_ => null)
            .ShowMessageOf<Refusal>(refusal => refusal.Message));

        Assert.Equal("квота", errors.Of(new QuotaRefusal("квота")));
    }

    [Fact]
    public void First_registration_wins()
    {
        var errors = Build(shell => shell
            .ShowMessageOf<QuotaRefusal>(_ => "Квота исчерпана")
            .ShowMessageOf<Refusal>(refusal => refusal.Message));

        Assert.Equal("Квота исчерпана", errors.Of(new QuotaRefusal("квота")));
    }

    [Fact]
    public void Extension_registers_through_the_service_collection()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.ShowMessageOf<Refusal>(refusal => refusal.Message);
        services.AddEnfilade();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        Assert.Equal("мест нет", scope.ServiceProvider.GetRequiredService<ErrorMessages>().Of(new Refusal("мест нет")));
    }

    private static ErrorMessages Build(Action<EnfiladeBuilder> configure)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        configure(services.AddEnfilade());
        return services.BuildServiceProvider().CreateScope().ServiceProvider.GetRequiredService<ErrorMessages>();
    }
}
