namespace Tenekon.MethodOverloads.AcceptanceCriterias.ExcludeAny;

public sealed class ExcludeAny_Class_7_Param_1;
public sealed class ExcludeAny_Class_7_Param_2;
public sealed class ExcludeAny_Class_7_Param_3;

public interface ExcludeAny_Class_7_Matcher
{
    [GenerateOverloads(ExcludeAny = [nameof(param_2)])]
    void Case_1(
        ExcludeAny_Class_7_Param_1 param_1,
        ExcludeAny_Class_7_Param_2 param_2,
        ExcludeAny_Class_7_Param_3 param_3);
}

/// <summary>
/// ExcludeAny on matcher methods is honored via matcher-based windows.
/// </summary>
[GenerateMethodOverloads(Matchers = [typeof(ExcludeAny_Class_7_Matcher)])]
public sealed class ExcludeAny_Class_7
{
    public void Case_1(
        ExcludeAny_Class_7_Param_1 param_1,
        ExcludeAny_Class_7_Param_2 param_2,
        ExcludeAny_Class_7_Param_3 param_3)
    {
    }
}

public static class ExcludeAny_Class_7_AcceptanceCriterias
{
    public static void Case_1(this ExcludeAny_Class_7 source)
    {
        source.Case_1(default, default, default);
    }

    public static void Case_1(this ExcludeAny_Class_7 source, ExcludeAny_Class_7_Param_1 param_1)
    {
        source.Case_1(param_1, default, default);
    }

    public static void Case_1(this ExcludeAny_Class_7 source, ExcludeAny_Class_7_Param_3 param_3)
    {
        source.Case_1(default, default, param_3);
    }
}
