namespace Tenekon.MethodOverloads.AcceptanceCriterias;

/// <summary>
/// Nested generic target type with generic method; overloads should include all type parameters.
/// </summary>
public sealed class Class_45<TOuter>
{
    public sealed class Class_45_Inner<TInner>
        where TInner : struct
    {
        [GenerateOverloads(nameof(param_b))]
        public TResult Case_1<TResult>(int param_a, TInner param_b)
            where TResult : class, new()
        {
            return new TResult();
        }
    }
}

public static class Class_45_AcceptanceCriterias
{
    public static TResult Case_1<TOuter, TInner, TResult>(
        this Class_45<TOuter>.Class_45_Inner<TInner> source,
        int param_a)
        where TInner : struct
        where TResult : class, new()
    {
        return source.Case_1<TResult>(param_a, default(TInner));
    }
}
