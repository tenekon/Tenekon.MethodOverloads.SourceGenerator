using System.Diagnostics.CodeAnalysis;

namespace Tenekon.MethodOverloads.AcceptanceCriterias.ExcludeAny;

public sealed class ExcludeAny_Class_4_Param_1;
public sealed class ExcludeAny_Class_4_Param_2;
public sealed class ExcludeAny_Class_4_Param_3;

/// <summary>
/// ExcludeAny references a parameter outside the window (MOG018).
/// </summary>
public sealed class ExcludeAny_Class_4
{
    [SuppressMessage("MethodOverloadsGenerator", "MOG018")]
    [GenerateOverloads(Begin = nameof(param_2), End = nameof(param_3), ExcludeAny = [nameof(param_1)])]
    public void Case_1(
        ExcludeAny_Class_4_Param_1 param_1,
        ExcludeAny_Class_4_Param_2 param_2,
        ExcludeAny_Class_4_Param_3 param_3)
    {
    }
}

public static class ExcludeAny_Class_4_AcceptanceCriterias
{
    // No extension methods expected due to invalid ExcludeAny entries.
}
