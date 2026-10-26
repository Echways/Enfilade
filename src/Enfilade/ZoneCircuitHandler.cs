using Microsoft.AspNetCore.Components.Server.Circuits;

namespace Enfilade;

internal sealed class ZoneCircuitHandler(ViewerZone zone) : CircuitHandler
{
    public override Func<CircuitInboundActivityContext, Task> CreateInboundActivityHandler(Func<CircuitInboundActivityContext, Task> next) =>
        context =>
        {
            Words.Zone = zone.Zone;
            return next(context);
        };
}
