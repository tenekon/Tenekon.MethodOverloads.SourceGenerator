using System.Collections.Immutable;

namespace Tenekon.MethodOverloads.SourceGenerator.Helpers;

internal readonly record struct EquatableArray<T>(ImmutableArray<T> Items)
{
    public static EquatableArray<T> Empty => new(ImmutableArray<T>.Empty);

    public bool Equals(EquatableArray<T> other)
    {
        var left = Items;
        var right = other.Items;
        if (left.Length != right.Length)
        {
            return false;
        }

        for (var i = 0; i < left.Length; i++)
        {
            if (!EqualityComparer<T>.Default.Equals(left[i], right[i]))
            {
                return false;
            }
        }

        return true;
    }

    public override int GetHashCode()
    {
        var hash = 17;
        foreach (var item in Items)
        {
            hash = (hash * 31) + (item is null ? 0 : item.GetHashCode());
        }

        return hash;
    }
}
