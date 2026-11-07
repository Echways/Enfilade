using System.Collections;

namespace Enfilade;

internal sealed class ListingParts<TPart>(Action<IReadOnlyList<TPart>, TPart>? admit = null) : IReadOnlyList<TPart>
    where TPart : class
{
    private readonly List<TPart> _parts = [];

    public int Count => _parts.Count;

    public TPart this[int index] => _parts[index];

    public TNext Place<TNext>(TNext? current, TNext next) where TNext : class, TPart
    {
        if (current is null)
        {
            admit?.Invoke(_parts, next);
            _parts.Add(next);
        }
        else
        {
            _parts[_parts.IndexOf(current)] = next;
        }

        return next;
    }

    public void Remove(TPart? part)
    {
        if (part is not null)
            _parts.Remove(part);
    }

    public IEnumerator<TPart> GetEnumerator() => _parts.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
