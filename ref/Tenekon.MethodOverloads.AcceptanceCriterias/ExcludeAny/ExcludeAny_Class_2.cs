namespace Tenekon.MethodOverloads.AcceptanceCriterias.ExcludeAny;

public sealed class ExcludeAny_Class_2_Param_1;
public sealed class ExcludeAny_Class_2_Param_2;
public sealed class ExcludeAny_Class_2_Param_3;
public sealed class ExcludeAny_Class_2_Param_4;

/// <summary>
/// ExcludeAny omits multiple parameters; overloads keep only the non-excluded ones.
/// </summary>
public sealed class ExcludeAny_Class_2
{
    [GenerateOverloads(Begin = nameof(param_1), End = nameof(param_4), ExcludeAny = [nameof(param_2), nameof(param_3)])]
    public void Case_1(
        ExcludeAny_Class_2_Param_1 param_1,
        ExcludeAny_Class_2_Param_2 param_2,
        ExcludeAny_Class_2_Param_3 param_3,
        ExcludeAny_Class_2_Param_4 param_4)
    {
    }
}

public static class ExcludeAny_Class_2_AcceptanceCriterias
{
    public static void Case_1(this ExcludeAny_Class_2 source)
    {
        source.Case_1(default, default, default, default);
    }

    public static void Case_1(this ExcludeAny_Class_2 source, ExcludeAny_Class_2_Param_1 param_1)
    {
        source.Case_1(param_1, default, default, default);
    }

    public static void Case_1(this ExcludeAny_Class_2 source, ExcludeAny_Class_2_Param_4 param_4)
    {
        source.Case_1(default, default, default, param_4);
    }
}
