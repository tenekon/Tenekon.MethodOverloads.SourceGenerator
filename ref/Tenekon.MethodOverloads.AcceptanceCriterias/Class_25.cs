using System.Diagnostics.CodeAnalysis;
using Tenekon.MethodOverloads.SourceGenerator;

namespace Tenekon.MethodOverloads.AcceptanceCriterias;

internal static class Class_25_MatcherUnused
{
    [SuppressMessage("MethodOverloadsGenerator", "MOG002")]
    [GenerateOverloads(Begin = nameof(param_1), End = nameof(param_1))]
    internal static extern void Matcher_1(Guid param_1);
}

/// <summary>
/// Unused matcher method should produce MOG002 once per matcher method.
/// </summary>
[GenerateMethodOverloads(Matchers = [typeof(Class_25_MatcherUnused)])]
public sealed class Class_25_Target
{
    public void Case_1(int param_1, string? param_2, bool param_3) { }
}

/// <summary>
/// Mixed matcher set: one matcher applies, one does not (MOG002 expected for the unused one).
/// </summary>
[GenerateMethodOverloads(Matchers = [typeof(Class_25_MatcherMixed)])]
public sealed class Class_25_MixedTarget
{
    public void Case_1(int param_1, string? param_2, bool param_3) { }
}

internal static class Class_25_MatcherMixed
{
    [GenerateOverloads(Begin = nameof(param_2), End = nameof(param_2))]
    internal static extern void Matcher_1(int param_1, string? param_2);

    [SuppressMessage("MethodOverloadsGenerator", "MOG002")]
    [GenerateOverloads(Begin = nameof(param_1), End = nameof(param_1))]
    internal static extern void Matcher_2(Guid param_1);
}

[SuppressMessage("ReSharper", "PreferConcreteValueOverDefault")]
/// <summary>
/// No overloads expected for Class_25_Target because its matcher does not match any target method.
/// </summary>
public static class Class_25_AcceptanceCriterias
{
    public static void Case_1(this Class_25_MixedTarget source, int param_1, bool param_3) =>
        source.Case_1(param_1, param_2: null, param_3);
}
