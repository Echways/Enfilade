namespace Enfilade.Tests.Hosts;

public class Refusal(string message) : Exception(message);

public sealed class QuotaRefusal(string message) : Refusal(message);
