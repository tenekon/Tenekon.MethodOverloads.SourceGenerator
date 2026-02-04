using Microsoft.CodeAnalysis;

namespace Tenekon.MethodOverloads.SourceGenerator.Helpers;

internal static class IncrementalValuesExtensions
{
    public static IncrementalValuesProvider<TResult> GroupBy<TSource, TKey, TResult>(
        this IncrementalValuesProvider<TSource> source,
        Func<TSource, TKey> keySelector,
        Func<TKey, IEnumerable<TSource>, TResult> resultSelector,
        IEqualityComparer<TKey>? keyComparer = null)
    {
        keyComparer ??= EqualityComparer<TKey>.Default;
        return source.Collect().SelectMany((values, _) => values.GroupBy(keySelector, resultSelector, keyComparer));
    }
}
