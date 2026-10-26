using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Enfilade;

public sealed class EnfiladeBuilder(IServiceCollection services)
{
    public IServiceCollection Services { get; } = services;

    public EnfiladeBuilder AddSection<TPage>(string title) where TPage : IComponent => AddSection<TPage>(() => title);

    public EnfiladeBuilder AddSection<TPage>(Func<string> title) where TPage : IComponent
    {
        var section = new Section(typeof(TPage), title);
        return Configure(options =>
        {
            options.Assemblies.Add(typeof(TPage).Assembly);
            options.Navigation.Add(section);
        });
    }

    public EnfiladeBuilder AddCaption(string text) => AddCaption(() => text);

    public EnfiladeBuilder AddCaption(Func<string> text) => Configure(options => options.Navigation.Add(new Caption(text)));

    public EnfiladeBuilder AddPages(Assembly assembly) => Configure(options => options.Assemblies.Add(assembly));

    public EnfiladeBuilder AddStylesheet(string path) => Configure(options => options.Stylesheets.Add(path));

    private EnfiladeBuilder Configure(Action<EnfiladeOptions> configure)
    {
        Services.Configure(configure);
        return this;
    }
}
