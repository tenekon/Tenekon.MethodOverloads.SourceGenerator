namespace Tenekon.MethodOverloads.AcceptanceCriterias;

/// <summary>
/// Two-parameter method using End/EndExclusive window boundaries.
/// Expects all unique subsequence overloads for the selected window.
/// </summary>
public abstract class Class_2
{
    [GenerateOverloads(EndExclusive = nameof(param_2))]
    public abstract void Case_1(bool param_1, CancellationToken param_2);

    [GenerateOverloads(End = nameof(param_1))]
    public abstract void Case_2(bool param_1, CancellationToken param_2);

    [GenerateOverloads(nameof(param_1))]
    public abstract void Case_3(bool param_1, CancellationToken param_2);

    [GenerateOverloads(Begin = nameof(param_1), EndExclusive = nameof(param_2))]
    public abstract void Case_4(bool param_1, CancellationToken param_2);

    [GenerateOverloads(Begin = nameof(param_2), End = nameof(param_1))]
    public abstract void Case_5(bool param_1, CancellationToken param_2);

    [GenerateOverloads(BeginExclusive = nameof(param_1), EndExclusive = nameof(param_2))]
    public abstract void Case_6(bool param_1, CancellationToken param_2);
}

/// <summary>
/// Expected extension overloads (or none) for Class_2 cases.
/// </summary>
public static class Class_2_AcceptanceCriterias
{
    public static void Case_1(this Class_2 source, CancellationToken param_2)
    {
        source.Case_1(param_1: false, param_2);
    }

    public static void Case_2(this Class_2 source, CancellationToken param_2)
    {
        source.Case_2(param_1: false, param_2);
    }

    public static void Case_3(this Class_2 source, CancellationToken param_2)
    {
        source.Case_3(param_1: false, param_2);
    }

    public static void Case_4(this Class_2 source, CancellationToken param_2)
    {
        source.Case_4(param_1: false, param_2);
    }

    // No extension methods for Case_5

    // No extension methods for Case_6
}