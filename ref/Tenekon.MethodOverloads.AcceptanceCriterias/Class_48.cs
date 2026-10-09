using System.Diagnostics.CodeAnalysis;

namespace Tenekon.MethodOverloads.AcceptanceCriterias;

// One matcher per path of the matcher validation step. Each matcher reports its error in the matcher type itself,
// whether or not a target uses it. The error kinds themselves are covered at targets (e.g. Class_21, Class_44,
// SupplyParameterType_Class_7). The matchers are abstract classes because the acceptance infrastructure maps
// diagnostics only inside classes. The last matcher is used by Class_48: its Matchers attribute is ignored (MOG021),
// its other attribute still applies.

public sealed class Class_48_Bucket;

public sealed class Class_48_Constraint;

public sealed class Class_48_Param<TConstraint>;

/// <summary>
/// Invalid window (MOG001). Stands for all window errors (MOG001, MOG007, MOG009, MOG010, MOG018, MOG019), which
/// share one check.
/// </summary>
[OverloadMatcher]
[SuppressMessage("MethodOverloadsGenerator", "MOG001")]
public abstract class Class_48_InvalidWindowMatcher
{
    [GenerateOverloads(Begin = "missing")]
    public abstract void Match(int value);
}

/// <summary>
/// Valid window with identical Begin and End (MOG008).
/// </summary>
[OverloadMatcher]
[SuppressMessage("MethodOverloadsGenerator", "MOG008")]
public abstract class Class_48_RedundantAnchorsMatcher
{
    [GenerateOverloads(Begin = nameof(value), End = nameof(value))]
    public abstract void Match(int value);
}

/// <summary>
/// Matcher method without parameters (MOG011).
/// </summary>
[OverloadMatcher]
[SuppressMessage("MethodOverloadsGenerator", "MOG011")]
public abstract class Class_48_ParameterlessMatcher
{
    [GenerateOverloads]
    public abstract void Match();
}

/// <summary>
/// Invalid bucket type (MOG013).
/// </summary>
[OverloadMatcher]
[SuppressMessage("MethodOverloadsGenerator", "MOG013")]
public abstract class Class_48_InvalidBucketMatcher
{
    [GenerateOverloads(nameof(value))]
    [OverloadGenerationOptions(BucketType = typeof(Class_48_Bucket))]
    public abstract void Match(int value);
}

/// <summary>
/// Invalid SupplyParameterType (MOG014). Stands for all supply errors (MOG014, MOG015, MOG016), which share one
/// check.
/// </summary>
[OverloadMatcher]
[SuppressMessage("MethodOverloadsGenerator", "MOG014")]
public abstract class Class_48_InvalidSupplyMatcher
{
    [GenerateOverloads(nameof(optionalObject))]
    [SupplyParameterType("TConstraint", typeof(Class_48_Constraint))]
    public abstract void Match<TConstraint>(Class_48_Param<TConstraint> param, object? optionalObject);
}

/// <summary>
/// Matchers on a matcher method are reported (MOG021) and ignored; the other attribute of the method still applies.
/// </summary>
[OverloadMatcher]
[SuppressMessage("MethodOverloadsGenerator", "MOG021")]
public abstract class Class_48_MatchersOnMatcherMethodMatcher
{
    [GenerateOverloads(nameof(param_b))]
    [GenerateOverloads(Matchers = [typeof(Class_48_InvalidWindowMatcher)])]
    public abstract void Match(int param_a, string? param_b);
}

/// <summary>
/// Uses the matcher above and gets the overload from its valid attribute.
/// </summary>
[GenerateMethodOverloads(Matchers = [typeof(Class_48_MatchersOnMatcherMethodMatcher)])]
public sealed class Class_48
{
    public void Case_1(int param_1, string? param_2) { }
}

/// <summary>
/// Only Class_48 gets overloads; matcher types are never targets.
/// </summary>
public static class Class_48_AcceptanceCriterias
{
    public static void Case_1(this Class_48 source, int param_1)
    {
        source.Case_1(param_1, param_2: null);
    }
}
