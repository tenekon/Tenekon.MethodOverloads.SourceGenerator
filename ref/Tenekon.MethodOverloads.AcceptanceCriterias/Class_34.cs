using System.Diagnostics.CodeAnalysis;

namespace Tenekon.MethodOverloads.AcceptanceCriterias;

internal interface Class_34_Matcher
{
    [GenerateOverloads(nameof(param_b))]
    void Matcher_1(int param_a, string? param_b);
}

public abstract class Class_34
{
    [SuppressMessage("MethodOverloadsGenerator", "MOG012")]
    [GenerateOverloads(Begin = nameof(param_2), Matchers = [typeof(Class_34_Matcher)])]
    public abstract void Case_1(int param_1, string? param_2);
}

public static class Class_34_AcceptanceCriterias
{
    // No overloads expected because the diagnostic blocks generation.
}
