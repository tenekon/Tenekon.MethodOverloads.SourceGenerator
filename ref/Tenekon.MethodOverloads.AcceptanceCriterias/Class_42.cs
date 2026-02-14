using Tenekon.MethodOverloads.SourceGenerator;

namespace Tenekon.MethodOverloads.AcceptanceCriterias;

/// <summary>
/// Nested generic target type; overloads should include all containing type parameters.
/// </summary>
public sealed class Class_42<TOuter>
{
    public sealed class Class_42_Inner<TInner>
        where TInner : struct
    {
        [GenerateOverloads(nameof(param_b))]
        public void Case_1(int param_a, TInner param_b)
        {
        }
    }
}

public static class Class_42_AcceptanceCriterias
{
    public static void Case_1<TOuter, TInner>(this Class_42<TOuter>.Class_42_Inner<TInner> source, int param_a)
        where TInner : struct
    {
        source.Case_1(param_a, default(TInner));
    }
}
