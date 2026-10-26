using Microsoft.Extensions.DependencyInjection;

namespace Enfilade;

public sealed class SectionSetup
{
    internal string? IconSvg { get; private set; }

    internal Func<IServiceProvider, Task<long>>? Counter { get; private set; }

    public SectionSetup Icon(string svg)
    {
        IconSvg = svg;
        return this;
    }

    public SectionSetup Badge<TService>(Func<TService, Task<long>> count) where TService : notnull
    {
        Counter = services => count(services.GetRequiredService<TService>());
        return this;
    }
}
