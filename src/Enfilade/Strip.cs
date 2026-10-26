namespace Enfilade;

public sealed record Strip(string Label, long Value, string Shown, string? Detail = null, bool Lit = false);
