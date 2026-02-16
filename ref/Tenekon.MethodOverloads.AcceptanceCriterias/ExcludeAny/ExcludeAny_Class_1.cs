namespace Tenekon.MethodOverloads.AcceptanceCriterias.ExcludeAny;

public sealed class ExcludeAny_Class_1_Param_1;
public sealed class ExcludeAny_Class_1_Param_2;
public sealed class ExcludeAny_Class_1_Param_3;

/// <summary>
/// ExcludeAny omits a middle parameter; only overloads that drop it are generated.
/// </summary>
public sealed class ExcludeAny_Class_1
{
    [GenerateOverloads(Begin = nameof(param_1), End = nameof(param_3), ExcludeAny = [nameof(param_2)])]
    public void Case_1(
        ExcludeAny_Class_1_Param_1 param_1,
        ExcludeAny_Class_1_Param_2 param_2,
        ExcludeAny_Class_1_Param_3 param_3)
    {
    }
}

public static class ExcludeAny_Class_1_AcceptanceCriterias
{
    public static void Case_1(this ExcludeAny_Class_1 source)
    {
        source.Case_1(default, default, default);
    }

    public static void Case_1(this ExcludeAny_Class_1 source, ExcludeAny_Class_1_Param_1 param_1)
    {
        source.Case_1(param_1, default, default);
    }

    public static void Case_1(this ExcludeAny_Class_1 source, ExcludeAny_Class_1_Param_3 param_3)
    {
        source.Case_1(default, default, param_3);
    }
}
