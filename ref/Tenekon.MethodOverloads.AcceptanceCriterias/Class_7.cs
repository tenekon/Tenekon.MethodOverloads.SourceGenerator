namespace Tenekon.MethodOverloads.AcceptanceCriterias;

/// <summary>
/// Matcher-based generation with opposite match mode overrides to Class_6.
/// Expects overloads only when effective match mode allows.
/// </summary>
[OverloadGenerationOptions(RangeAnchorMatchMode = RangeAnchorMatchMode.TypeOnly)]
internal interface Class_7_Matcher
{
    [OverloadGenerationOptions(RangeAnchorMatchMode = RangeAnchorMatchMode.TypeAndName)]
    [GenerateOverloads(EndExclusive = nameof(param_2))]
    void Matcher_1(bool param_1, CancellationToken param_2);
}

[OverloadGenerationOptions(RangeAnchorMatchMode = RangeAnchorMatchMode.TypeAndName)]
public abstract class Class_7
{
    [GenerateOverloads(Matchers = [typeof(Class_7_Matcher)])]
    public abstract void Case_1(bool param_1_mismatch, CancellationToken param_2_mismatch);

    [OverloadGenerationOptions(RangeAnchorMatchMode = RangeAnchorMatchMode.TypeOnly)]
    [GenerateOverloads(Matchers = [typeof(Class_7_Matcher)])]
    public abstract void Case_2(bool param_1, CancellationToken param_2);
}

/// <summary>
/// Expected extension overloads (or none) for Class_7 cases.
/// </summary>
public static class Class_7_AcceptanceCriterias
{
    // No extension methods for Case_1

    public static void Case_2(this Class_7 source, CancellationToken param_2)
    {
        source.Case_2(param_1: false, param_2);
    }
}