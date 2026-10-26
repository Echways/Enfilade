namespace Enfilade.Tests.Hosts;

public sealed class Inbox
{
    public long Count { get; set; }

    public int Calls { get; private set; }

    public Task<long> CountAsync()
    {
        Calls++;
        return Task.FromResult(Count);
    }
}

public sealed class BrokenCounter
{
    public Task<long> CountAsync() => throw new InvalidOperationException("the store is down");
}
