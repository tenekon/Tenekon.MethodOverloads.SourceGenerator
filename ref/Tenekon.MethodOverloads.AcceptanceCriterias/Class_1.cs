using Tenekon.MethodOverloads.SourceGenerator;

namespace Tenekon.MethodOverloads.AcceptanceCriterias;

/// <summary>
/// Two-parameter method with various Begin/End window combinations.
/// Expects all unique subsequence overloads within the selected window.
/// </summary>
public abstract class Class_1
{
    [GenerateOverloads]
    public abstract void Case_1(bool param_1, string? param_2);

    [GenerateOverloads(Begin = nameof(param_1))]
    public abstract void Case_2(bool param_1, string? param_2);

    [GenerateOverloads(Begin = nameof(param_1), End = nameof(param_2))]
    public abstract void Case_3(bool param_1, string? param_2);

    [GenerateOverloads(End = nameof(param_2))]
    public abstract void Case_4(bool param_1, string? param_2);

    [GenerateOverloads(End = nameof(param_2))]
    public abstract void Case_5(bool param_1, string? param_2);

    [GenerateOverloads(Begin = nameof(param_2))]
    public abstract void Case_6(bool param_1, string? param_2);

    [GenerateOverloads(nameof(cancellationToken))]
    public abstract void Case_7(CancellationToken cancellationToken);
}

/// <summary>
/// Expected extension overloads (or none) for Class_1 cases.
/// </summary>
public static class Class_1_AcceptanceCriterias
{
    public static void Case_1(this Class_1 source, bool param_1)
    {
        source.Case_1(param_1, param_2: null);
    }

    public static void Case_1(this Class_1 source, string? param_2)
    {
        source.Case_1(param_1: false, param_2);
    }

    public static void Case_1(this Class_1 source)
    {
        source.Case_1(param_1: false, param_2: null);
    }

    public static void Case_2(this Class_1 source, bool param_1)
    {
        source.Case_2(param_1, param_2: null);
    }

    public static void Case_2(this Class_1 source, string? param_2)
    {
        source.Case_2(param_1: false, param_2);
    }

    public static void Case_2(this Class_1 source)
    {
        source.Case_2(param_1: false, param_2: null);
    }

    public static void Case_3(this Class_1 source, bool param_1)
    {
        source.Case_3(param_1, param_2: null);
    }

    public static void Case_3(this Class_1 source, string? param_2)
    {
        source.Case_3(param_1: false, param_2);
    }

    public static void Case_3(this Class_1 source)
    {
        source.Case_3(param_1: false, param_2: null);
    }

    public static void Case_4(this Class_1 source, bool param_1)
    {
        source.Case_4(param_1, param_2: null);
    }

    public static void Case_4(this Class_1 source, string? param_2)
    {
        source.Case_4(param_1: false, param_2);
    }

    public static void Case_4(this Class_1 source)
    {
        source.Case_4(param_1: false, param_2: null);
    }

    public static void Case_5(this Class_1 source, bool param_1)
    {
        source.Case_5(param_1, param_2: null);
    }

    public static void Case_5(this Class_1 source, string? param_2)
    {
        source.Case_5(param_1: false, param_2);
    }

    public static void Case_5(this Class_1 source)
    {
        source.Case_5(param_1: false, param_2: null);
    }

    public static void Case_6(this Class_1 source, bool param_1)
    {
        source.Case_6(param_1, param_2: null);
    }

    public static void Case_7(this Class_1 source)
    {
        source.Case_7(cancellationToken: default);
    }
}