namespace Enfilade;

public sealed record TimelineEntry(DateTimeOffset At, string Text, string? Who = null, Tone Tone = Tone.Neutral);
