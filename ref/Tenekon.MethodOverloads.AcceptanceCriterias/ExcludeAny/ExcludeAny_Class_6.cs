using System.Diagnostics.CodeAnalysis;

namespace Tenekon.MethodOverloads.AcceptanceCriterias.ExcludeAny;

public sealed class ExcludeAny_Class_6_Param_1;
public sealed class ExcludeAny_Class_6_Param_2;

public interface ExcludeAny_Class_6_Matcher
{
    [GenerateOverloads(nameof(param_2))]
    void Case_1(ExcludeAny_Class_6_Param_1 param_1, ExcludeAny_Class_6_Param_2 param_2);
}

/// <summary>
/// ExcludeAny combined with Matchers on the target attribute triggers MOG017.
/// </summary>
public sealed class ExcludeAny_Class_6
{
    [SuppressMessage("MethodOverloadsGenerator", "MOG017")]
    [GenerateOverloads(Matchers = [typeof(ExcludeAny_Class_6_Matcher)], ExcludeAny = [nameof(param_2)])]
    public void Case_1(ExcludeAny_Class_6_Param_1 param_1, ExcludeAny_Class_6_Param_2 param_2)
    {
    }
}

public static class ExcludeAny_Class_6_AcceptanceCriterias
{
    // No extension methods expected due to ExcludeAny + Matchers conflict.
}
