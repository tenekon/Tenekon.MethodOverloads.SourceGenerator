namespace Tenekon.MethodOverloads.AcceptanceCriterias.ExcludeAny;

public sealed class ExcludeAny_Class_3_Param_1;
public sealed class ExcludeAny_Class_3_Param_2;

/// <summary>
/// ExcludeAny covers the entire window, so no overloads are generated.
/// </summary>
public sealed class ExcludeAny_Class_3
{
    [GenerateOverloads(Begin = nameof(param_1), End = nameof(param_2), ExcludeAny = [nameof(param_1), nameof(param_2)])]
    public void Case_1(ExcludeAny_Class_3_Param_1 param_1, ExcludeAny_Class_3_Param_2 param_2)
    {
    }
}

public static class ExcludeAny_Class_3_AcceptanceCriterias
{
    // No extension methods expected when ExcludeAny covers the entire window.
}
