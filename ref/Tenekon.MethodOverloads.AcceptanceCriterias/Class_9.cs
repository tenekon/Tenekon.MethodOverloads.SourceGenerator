using System.Diagnostics.CodeAnalysis;
using Tenekon.MethodOverloads.SourceGenerator;

namespace Tenekon.MethodOverloads.AcceptanceCriterias;

/// <summary>
/// Optional parameters only in the middle; required params on both sides.
/// Expects all unique subsequences within the middle span.
/// </summary>
public abstract class Class_9
{
    [GenerateOverloads(Begin = nameof(param_2), End = nameof(param_3))]
    public abstract void Case_1(int param_1, string? param_2, bool param_3, int param_4);
}

/// <summary>
/// Expected extension overloads for all unique subsequences of the optional span.
/// </summary>
[SuppressMessage("ReSharper", "PreferConcreteValueOverDefault")]
public static class Class_9_AcceptanceCriterias
{
    public static void Case_1(this Class_9 source, int param_1, string? param_2, int param_4) =>
        source.Case_1(param_1, param_2, param_3: false, param_4);

    public static void Case_1(this Class_9 source, int param_1, bool param_3, int param_4) =>
        source.Case_1(param_1, param_2: null, param_3: param_3, param_4);

    public static void Case_1(this Class_9 source, int param_1, int param_4) =>
        source.Case_1(param_1, param_2: null, param_3: false, param_4);
}



