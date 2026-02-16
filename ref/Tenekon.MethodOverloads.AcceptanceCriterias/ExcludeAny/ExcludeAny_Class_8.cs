using System.Diagnostics.CodeAnalysis;

namespace Tenekon.MethodOverloads.AcceptanceCriterias.ExcludeAny;

public sealed class ExcludeAny_Class_8_Param_1;
public sealed class ExcludeAny_Class_8_Param_2;
public sealed class ExcludeAny_Class_8_Param_3;

/// <summary>
/// Defaults inside the window still block generation even with ExcludeAny (MOG003).
/// </summary>
public sealed class ExcludeAny_Class_8
{
    [SuppressMessage("MethodOverloadsGenerator", "MOG003")]
    [GenerateOverloads(Begin = nameof(param_1), End = nameof(param_3), ExcludeAny = [nameof(param_1)])]
    public void Case_1(
        ExcludeAny_Class_8_Param_1 param_1,
        ExcludeAny_Class_8_Param_2? param_2 = null,
        ExcludeAny_Class_8_Param_3? param_3 = null)
    {
    }
}

public static class ExcludeAny_Class_8_AcceptanceCriterias
{
    // No extension methods expected due to defaults inside window.
}
