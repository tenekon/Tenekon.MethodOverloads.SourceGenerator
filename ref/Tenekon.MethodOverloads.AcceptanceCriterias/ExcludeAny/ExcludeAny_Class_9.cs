using System.Diagnostics.CodeAnalysis;

namespace Tenekon.MethodOverloads.AcceptanceCriterias.ExcludeAny;

public sealed class ExcludeAny_Class_9_Param_1;
public sealed class ExcludeAny_Class_9_Param_2;
public sealed class ExcludeAny_Class_9_Param_3;

/// <summary>
/// Params outside the window still block generation with ExcludeAny (MOG004).
/// </summary>
public sealed class ExcludeAny_Class_9
{
    [SuppressMessage("MethodOverloadsGenerator", "MOG004")]
    [GenerateOverloads(Begin = nameof(param_1), End = nameof(param_2), ExcludeAny = [nameof(param_1)])]
    public void Case_1(
        ExcludeAny_Class_9_Param_1 param_1,
        ExcludeAny_Class_9_Param_2 param_2,
        params ExcludeAny_Class_9_Param_3[] param_3)
    {
    }
}

public static class ExcludeAny_Class_9_AcceptanceCriterias
{
    // No extension methods expected due to params outside window.
}
