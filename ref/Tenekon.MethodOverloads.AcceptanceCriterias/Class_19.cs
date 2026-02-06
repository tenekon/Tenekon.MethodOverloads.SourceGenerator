using Tenekon.MethodOverloads.SourceGenerator;

namespace Tenekon.MethodOverloads.AcceptanceCriterias;

internal interface Class_19_Matcher
{
    [GenerateOverloads(nameof(param_2))]
    void Matcher_1(int param_1, string? param_2);
}

/// <summary>
/// Matcher parameter count differs; subsequence matching is allowed.
/// </summary>
[GenerateMethodOverloads(Matchers = [typeof(Class_19_Matcher)])]
public abstract class Class_19
{
    public abstract void Case_1(int param_1, string? param_2, bool param_3);
}

/// <summary>
/// Expected extension overloads when matcher is a subsequence.
/// </summary>
public static class Class_19_AcceptanceCriterias
{
    public static void Case_1(this Class_19 source, int param_1, bool param_3)
    {
        source.Case_1(param_1, param_2: null, param_3);
    }
}