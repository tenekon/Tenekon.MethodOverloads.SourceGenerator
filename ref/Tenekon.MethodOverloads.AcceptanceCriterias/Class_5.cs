using System.Diagnostics.CodeAnalysis;
using Tenekon.MethodOverloads.SourceGenerator;

namespace Tenekon.MethodOverloads.AcceptanceCriterias;

/// <summary>
/// Matcher-based generation with mixed parameter name/type matching.
/// Expects overloads only when the selected match mode fits.
/// </summary>
[OverloadGenerationOptions(RangeAnchorMatchMode = RangeAnchorMatchMode.TypeOnly)]
[SuppressMessage("MethodOverloadsGenerator", "MOG002")]
internal class Class_5_Matcher
{
    [GenerateOverloads(EndExclusive = nameof(param_2))]
    internal static extern void Matcher_1(bool param_1, CancellationToken param_2);

    [OverloadGenerationOptions(RangeAnchorMatchMode = RangeAnchorMatchMode.TypeAndName)]
    [GenerateOverloads(EndExclusive = nameof(param_2))]
    internal static extern void Matcher_2(string? param_1, bool param_2, CancellationToken param_3);
}

[GenerateMethodOverloads(Matchers = [typeof(Class_5_Matcher)])]
public abstract class Class_5
{
    public abstract void Case_1(bool param_1, CancellationToken param_2);

    public abstract void Case_2(string? param_1_mismatch, bool param_2_mismatch, CancellationToken param_3_mismatch);
}

/// <summary>
/// Expected extension overloads (or none) for Class_5 cases.
/// </summary>
[SuppressMessage("ReSharper", "PreferConcreteValueOverDefault")]
public static class Class_5_AcceptanceCriterias
{
    public static void Case_1(this Class_5 source, CancellationToken cancellationToken) =>
        source.Case_1(param_1: false, cancellationToken);

    // No extension methods for Case_2
}


