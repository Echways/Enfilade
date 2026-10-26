namespace Enfilade.Tests.Hosts;

public sealed class OrderStore
{
    public int Calls { get; private set; }

    public Task<ListPage<Order>> PageAsync(ListQuery<Order> query, CancellationToken token)
    {
        Calls++;
        return Order.PageAsync(query, token);
    }
}
