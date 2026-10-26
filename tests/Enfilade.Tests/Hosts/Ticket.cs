namespace Enfilade.Tests.Hosts;

public sealed class Ticket(int id)
{
    public int Id { get; } = id;

    public string Title => $"Заявка {Id}";

    public static Task<ListPage<Ticket>> PageAsync(ListQuery<Ticket> query, CancellationToken token) =>
        Task.FromResult(Enumerable.Range(1, 3).Select(id => new Ticket(id)).AsQueryable().ToPage(query));
}
