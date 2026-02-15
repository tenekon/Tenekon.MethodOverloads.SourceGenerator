namespace Tenekon.MethodOverloads.AcceptanceCriterias;

/// <summary>
/// Generic target type; overloads should include containing type parameters.
/// </summary>
public sealed class Class_41<T> where T : class, new()
{
    [GenerateOverloads(nameof(param_b))]
    public void Case_1(int param_a, T param_b)
    {
    }
}

public static class Class_41_AcceptanceCriterias
{
    public static void Case_1<T>(this Class_41<T> source, int param_a) where T : class, new()
    {
        source.Case_1(param_a, default(T));
    }
}