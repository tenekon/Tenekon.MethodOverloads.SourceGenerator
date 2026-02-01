using System.Diagnostics.CodeAnalysis;
using Tenekon.MethodOverloads.SourceGenerator;

namespace Tenekon.MethodOverloads.AcceptanceCriterias;

/// <summary>
/// Multi-parameter optional span (3 params) with all unique subsequences expected.
/// </summary>
public abstract class Class_8
{
    [GenerateOverloads(Begin = nameof(param_2), End = nameof(param_4))]
    public abstract void Case_1(int param_1, string? param_2, bool param_3, CancellationToken param_4);
}

/// <summary>
/// Expected extension overloads for all unique subsequences of the optional span.
/// </summary>
[SuppressMessage("ReSharper", "PreferConcreteValueOverDefault")]
public static class Class_8_AcceptanceCriterias
{
    public static void Case_1(this Class_8 source, int param_1, string? param_2, bool param_3) =>
        source.Case_1(param_1, param_2, param_3, param_4: default);

    public static void Case_1(this Class_8 source, int param_1, string? param_2, CancellationToken param_4) =>
        source.Case_1(param_1, param_2, param_3: false, param_4: param_4);

    public static void Case_1(this Class_8 source, int param_1, string? param_2) =>
        source.Case_1(param_1, param_2, param_3: false, param_4: default);

    public static void Case_1(this Class_8 source, int param_1, bool param_3, CancellationToken param_4) =>
        source.Case_1(param_1, param_2: null, param_3: param_3, param_4: param_4);

    public static void Case_1(this Class_8 source, int param_1, bool param_3) =>
        source.Case_1(param_1, param_2: null, param_3: param_3, param_4: default);

    public static void Case_1(this Class_8 source, int param_1, CancellationToken param_4) =>
        source.Case_1(param_1, param_2: null, param_3: false, param_4: param_4);

    public static void Case_1(this Class_8 source, int param_1) =>
        source.Case_1(param_1, param_2: null, param_3: false, param_4: default);
}



