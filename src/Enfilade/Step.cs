namespace Enfilade;

public sealed record Step(string Title, StepState State, string? Detail = null);
