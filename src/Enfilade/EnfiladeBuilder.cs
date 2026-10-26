using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Enfilade;

public sealed class EnfiladeBuilder(IServiceCollection services)
{
    public IServiceCollection Services { get; } = services;

    public EnfiladeBuilder AddSection<TPage>(string title) where TPage : IComponent => AddSection<TPage>(() => title);

    public EnfiladeBuilder AddSection<TPage>(Func<string> title) where TPage : IComponent => AddSection<TPage>(title, _ => { });

    public EnfiladeBuilder AddSection<TPage>(string title, Action<SectionSetup> setup) where TPage : IComponent =>
        AddSection<TPage>(() => title, setup);

    public EnfiladeBuilder AddSection<TPage>(Func<string> title, Action<SectionSetup> setup) where TPage : IComponent
    {
        var details = new SectionSetup();
        setup(details);
        var section = new Section(typeof(TPage), title) { Icon = details.IconSvg, Badge = details.Counter };
        return Configure(options =>
        {
            options.Assemblies.Add(typeof(TPage).Assembly);
            options.Navigation.Add(section);
        });
    }

    public EnfiladeBuilder AddCaption(string text) => AddCaption(() => text);

    public EnfiladeBuilder AddCaption(Func<string> text) => Configure(options => options.Navigation.Add(new Caption(text)));

    public EnfiladeBuilder AddAccountLink(string title, string href) => AddAccountLink(() => title, href);

    public EnfiladeBuilder AddAccountLink(Func<string> title, string href) =>
        Configure(options => options.AccountLinks.Add(new AccountLink(title, href)));

    public EnfiladeBuilder AddPages(Assembly assembly) => Configure(options => options.Assemblies.Add(assembly));

    public EnfiladeBuilder AddStylesheet(string path) => Configure(options => options.Stylesheets.Add(path));

    private EnfiladeBuilder Configure(Action<EnfiladeOptions> configure)
    {
        Services.Configure(configure);
        return this;
    }
}
