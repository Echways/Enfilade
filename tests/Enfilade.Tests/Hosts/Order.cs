using System.ComponentModel.DataAnnotations;

namespace Enfilade.Tests.Hosts;

public enum OrderStatus
{
    New,
    [Display(Name = "Оплачен")]
    Paid,
}

public sealed record Order(int Id, string Customer, decimal Total, OrderStatus Status)
{
    public static readonly IReadOnlyList<Order> Sixty =
        [.. Enumerable.Range(1, 60).Select(id => new Order(id, $"Клиент {id:00}", id * 10m, id % 2 == 0 ? OrderStatus.Paid : OrderStatus.New))];

    public static Task<ListPage<Order>> PageAsync(ListQuery<Order> query, CancellationToken token) =>
        Task.FromResult(Sixty.AsQueryable().ToPage(query));
}
