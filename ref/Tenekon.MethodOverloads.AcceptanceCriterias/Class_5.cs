using System.Diagnostics.CodeAnalysis;
using Tenekon.MethodOverloads.SourceGenerator;

namespace Tenekon.MethodOverloads.AcceptanceCriterias;

/// <summary>
/// Matcher-based generation with mixed parameter name/type matching.
/// Expects overloads only when the selected match mode fits.
/// </summary>
[OverloadGenerationOptions(RangeAnchorMatchMode = RangeAnchorMatchMode.TypeOnly)]
internal interface Class_5_Matcher
{
    [GenerateOverloads(EndExclusive = nameof(param_2))]
    void Matcher_1(bool param_1, CancellationToken param_2);

    [OverloadGenerationOptions(RangeAnchorMatchMode = RangeAnchorMatchMode.TypeAndName)]
    [SuppressMessage("MethodOverloadsGenerator", "MOG002")]
    [GenerateOverloads(EndExclusive = nameof(param_2))]
    void Matcher_2(string? param_1, bool param_2, CancellationToken param_3);
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
public static class Class_5_AcceptanceCriterias
{
    public static void Case_1(this Class_5 source, CancellationToken cancellationToken)
    {
        source.Case_1(param_1: false, cancellationToken);
    }

    public static void Case_2(this Class_5 source, string? param_1_mismatch, CancellationToken param_3_mismatch)
    {
        source.Case_2(param_1_mismatch, param_2_mismatch: false, param_3_mismatch);
    }
}