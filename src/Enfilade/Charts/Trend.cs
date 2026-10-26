namespace Enfilade;

public sealed record Reading(DateTimeOffset At, double Value);

public sealed record Trend(string Name, IReadOnlyList<Reading> Readings);
