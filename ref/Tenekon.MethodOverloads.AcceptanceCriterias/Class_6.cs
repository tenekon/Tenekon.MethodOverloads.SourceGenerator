using System.Diagnostics.CodeAnalysis;
using Tenekon.MethodOverloads.SourceGenerator;

namespace Tenekon.MethodOverloads.AcceptanceCriterias;

/// <summary>
/// Matcher-based generation with class/method match mode overrides.
/// Expects overloads only when effective match mode allows.
/// </summary>
[OverloadGenerationOptions(RangeAnchorMatchMode = RangeAnchorMatchMode.TypeAndName)]
[SuppressMessage("MethodOverloadsGenerator", "MOG002")]
internal class Class_6_Matcher
{
    [OverloadGenerationOptions(RangeAnchorMatchMode = RangeAnchorMatchMode.TypeOnly)]
    [GenerateOverloads(EndExclusive = nameof(param_2))]
    internal static extern void Matcher_1(bool param_1, CancellationToken param_2);
}

[OverloadGenerationOptions(RangeAnchorMatchMode = RangeAnchorMatchMode.TypeAndName)]
public abstract class Class_6
{
    [GenerateOverloads(Matchers = [typeof(Class_6_Matcher)])]
    public abstract void Case_1(bool value, CancellationToken cancellationToken);

    [OverloadGenerationOptions(RangeAnchorMatchMode = RangeAnchorMatchMode.TypeOnly)]
    [GenerateOverloads(Matchers = [typeof(Class_6_Matcher)])]
    public abstract void Case_2(bool value, CancellationToken cancellationToken);
}

/// <summary>
/// Expected extension overloads (or none) for Class_6 cases.
/// </summary>
[SuppressMessage("ReSharper", "PreferConcreteValueOverDefault")]
public static class Class_6_AcceptanceCriterias
{
    // No extension methods for Case_1

    public static void Case_2(this Class_6 source, CancellationToken cancellationToken) =>
        source.Case_2(value: false, cancellationToken);
}


