using System.Diagnostics.CodeAnalysis;

namespace Tenekon.MethodOverloads.AcceptanceCriterias.ExcludeAny;

public sealed class ExcludeAny_Class_5_Param_1;

/// <summary>
/// ExcludeAny contains invalid entries (MOG019).
/// </summary>
public sealed class ExcludeAny_Class_5
{
    [SuppressMessage("MethodOverloadsGenerator", "MOG019")]
    [GenerateOverloads(ExcludeAny = [default(string)])]
    public void Case_1(ExcludeAny_Class_5_Param_1 param_1)
    {
    }
}

public static class ExcludeAny_Class_5_AcceptanceCriterias
{
    // No extension methods expected due to invalid ExcludeAny entries.
}
