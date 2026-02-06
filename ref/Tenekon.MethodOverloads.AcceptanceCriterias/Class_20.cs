using System.Diagnostics.CodeAnalysis;
using Tenekon.MethodOverloads.SourceGenerator;

namespace Tenekon.MethodOverloads.AcceptanceCriterias;

internal interface Class_20_Matcher
{
    [GenerateOverloads(nameof(param_b))]
    void Matcher_1(int param_a, string? param_b);
}

/// <summary>
/// Matcher subsequence appears twice, separated by a non-matching parameter.
/// Expect overloads for each matched window, with de-duplication by signature.
/// </summary>
[GenerateMethodOverloads(Matchers = [typeof(Class_20_Matcher)])]
public abstract class Class_20
{
    [SuppressMessage("MethodOverloadsGenerator", "MOG006")]
    public abstract void Case_1(
        int param_1,
        string? param_2,
        bool param_3,
        int param_4,
        string? param_5,
        CancellationToken param_6);
}

/// <summary>
/// Expected extension overloads for multiple matched subsequences.
/// </summary>
public static class Class_20_AcceptanceCriterias
{
    public static void Case_1(
        this Class_20 source,
        int param_1,
        bool param_3,
        int param_4,
        string? param_5,
        CancellationToken param_6)
    {
        source.Case_1(param_1, param_2: null, param_3, param_4, param_5, param_6);
    }

    public static void Case_1(
        this Class_20 source,
        int param_1,
        string? param_2,
        bool param_3,
        int param_4,
        CancellationToken param_6)
    {
        source.Case_1(param_1, param_2, param_3, param_4, param_5: null, param_6);
    }

    public static void Case_1(this Class_20 source, int param_1, bool param_3, int param_4, CancellationToken param_6)
    {
        source.Case_1(param_1, param_2: null, param_3, param_4, param_5: null, param_6);
    }
}
