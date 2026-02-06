using Tenekon.MethodOverloads.SourceGenerator;

namespace Tenekon.MethodOverloads.AcceptanceCriterias;

/// <summary>
/// Matcher type A for matcher-union testing.
/// </summary>
internal interface Class_36_MatcherA
{
    /// <summary>
    /// Optional window anchored at param_2.
    /// </summary>
    [GenerateOverloads(nameof(param_2))]
    void Matcher_1(int param_1, string? param_2);
}

/// <summary>
/// Matcher type B for matcher-union testing.
/// </summary>
internal interface Class_36_MatcherB
{
    /// <summary>
    /// Optional window anchored at param_3.
    /// </summary>
    [GenerateOverloads(nameof(param_3))]
    void Matcher_1(int param_1, string? param_2, bool param_3);
}

/// <summary>
/// Multiple Matchers-only GenerateOverloads attributes on a single method.
/// </summary>
public abstract class Class_36
{
    /// <summary>
    /// Target method using two matcher groups that should be unioned.
    /// </summary>
    [GenerateOverloads(Matchers = [typeof(Class_36_MatcherA)])]
    [GenerateOverloads(Matchers = [typeof(Class_36_MatcherB)])]
    public abstract void Case_1(int param_1, string? param_2, bool param_3);
}

public static class Class_36_AcceptanceCriterias
{
    public static void Case_1(this Class_36 source, int param_1, bool param_3) =>
        source.Case_1(param_1, param_2: null, param_3);

    public static void Case_1(this Class_36 source, int param_1, string? param_2) =>
        source.Case_1(param_1, param_2, param_3: default(bool));
}
