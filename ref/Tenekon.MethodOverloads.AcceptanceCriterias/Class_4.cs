using Tenekon.MethodOverloads.SourceGenerator;

namespace Tenekon.MethodOverloads.AcceptanceCriterias;

internal interface Class_4_Matcher
{
    [GenerateOverloads(EndExclusive = nameof(param_2))]
    void Matcher_2(bool param_1, CancellationToken param_2);
    
    [GenerateOverloads(EndExclusive = nameof(param_2))]
    void Matcher_2(string? param_1, bool param_2, CancellationToken param_3);
}

/// <summary>
/// Matcher-based generation with multiple matcher overloads.
/// Expects all unique subsequence overloads for matched methods.
/// </summary>
[GenerateMethodOverloads(Matchers = [typeof(Class_4_Matcher)])]
public abstract class Class_4
{
    public abstract void Case_1(bool param_1, CancellationToken param_2);

    public abstract void Case_2(string? param_1, bool param_2, CancellationToken param_3);

    public abstract void Case_3(string? param_1, bool param_2, CancellationToken param_3);
}

/// <summary>
/// Expected extension overloads (or none) for Class_4 cases.
/// </summary>
public static class Class_4_AcceptanceCriterias
{
    public static void Case_1(this Class_4 source, CancellationToken param_2) => source.Case_1(param_1: false, param_2);

    public static void Case_2(this Class_4 source, string? param_1, CancellationToken param_3) =>
        source.Case_2(param_1, param_2: false, param_3: param_3);

    public static void Case_2(this Class_4 source, bool param_2, CancellationToken param_3) =>
        source.Case_2(param_1: null, param_2, param_3);

    public static void Case_3(this Class_4 source, string? param_1, CancellationToken param_3) =>
        source.Case_3(param_1, param_2: false, param_3: param_3);

    public static void Case_3(this Class_4 source, bool param_2, CancellationToken param_3) =>
        source.Case_3(param_1: null, param_2, param_3);
}



