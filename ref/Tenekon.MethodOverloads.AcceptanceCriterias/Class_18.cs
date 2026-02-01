using System.Diagnostics.CodeAnalysis;
using Tenekon.MethodOverloads.SourceGenerator;

namespace Tenekon.MethodOverloads.AcceptanceCriterias;

[OverloadGenerationOptions(RangeAnchorMatchMode = RangeAnchorMatchMode.TypeAndName)]
[SuppressMessage("MethodOverloadsGenerator", "MOG002")]
internal class Class_18_Matcher
{
    [GenerateOverloads(Begin = nameof(param_2), End = nameof(param_2))]
    internal static extern void Matcher_1(int param_1, string? param_2);

    [GenerateOverloads(Begin = nameof(param_2), End = nameof(param_2))]
    internal static extern void Matcher_2(int param_1, string? param_2);
}

/// <summary>
/// TypeAndName matching with aliasing and casing differences.
/// </summary>
[GenerateMethodOverloads(Matchers = [typeof(Class_18_Matcher)])]
public abstract class Class_18
{
    [SuppressMessage("MethodOverloadsGenerator", "MOG006")]
    public abstract void Case_1(Int32 param_1, string? param_2);

    public abstract void Case_2(int Param_1, string? param_2);
}

/// <summary>
/// Expected extension overloads for TypeAndName matching.
/// </summary>
[SuppressMessage("ReSharper", "PreferConcreteValueOverDefault")]
public static class Class_18_AcceptanceCriterias
{
    public static void Case_1(this Class_18 source, int param_1) =>
        source.Case_1(param_1, param_2: null);

    // No extension methods for Case_2 (name casing mismatch).
}


