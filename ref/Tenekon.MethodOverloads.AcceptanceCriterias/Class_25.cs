using System.Diagnostics.CodeAnalysis;

namespace Tenekon.MethodOverloads.AcceptanceCriterias;

[OverloadMatcher]
internal interface Class_25_MatcherUnused
{
    [GenerateOverloads(nameof(param_a))]
    void Matcher_1(Guid param_a);
}

/// <summary>
/// Matcher method without a match in this target produces MOG002 at the typeof reference.
/// </summary>
[SuppressMessage("MethodOverloadsGenerator", "MOG002")]
[GenerateMethodOverloads(Matchers = [typeof(Class_25_MatcherUnused)])]
public sealed class Class_25_Target
{
    public void Case_1(int param_1, string? param_2, bool param_3) { }
}

/// <summary>
/// Mixed matcher set: one matcher applies, one does not (MOG002 expected for the unused one).
/// </summary>
[SuppressMessage("MethodOverloadsGenerator", "MOG002")]
[GenerateMethodOverloads(Matchers = [typeof(Class_25_MatcherMixed)])]
public sealed class Class_25_MixedTarget
{
    public void Case_1(int param_1, string? param_2, bool param_3) { }
}

[OverloadMatcher]
internal interface Class_25_MatcherMixed
{
    [GenerateOverloads(nameof(param_b))]
    void Matcher_1(int param_a, string? param_b);

    [GenerateOverloads(nameof(param_a))]
    void Matcher_2(Guid param_a);
}

/// <summary>
/// No overloads expected for Class_25_Target because its matcher does not match any target method.
/// </summary>
public static class Class_25_AcceptanceCriterias
{
    public static void Case_1(this Class_25_MixedTarget source, int param_1, bool param_3)
    {
        source.Case_1(param_1, param_2: null, param_3);
    }
}
