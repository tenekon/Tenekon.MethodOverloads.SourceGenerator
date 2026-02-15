namespace Tenekon.MethodOverloads.AcceptanceCriterias;

public class Class_33_Type_1;

public class Class_33_Type_2;

public class Class_33_Type_3;

public abstract class Class_33
{
    [GenerateOverloads]
    public abstract void Case_1(Class_33_Type_1? param_1, Class_33_Type_2? param_2);

    [GenerateOverloads(nameof(param_4))]
    public abstract void Case_1(Class_33_Type_3 param_3, Class_33_Type_1? param_4);
}

public static class Class_33_AcceptanceCriterias
{
    public static void Case_1(this Class_33 source, Class_33_Type_2? param_2)
    {
        source.Case_1(param_1: default, param_2);
    }

    public static void Case_1(this Class_33 source, Class_33_Type_1? param_1)
    {
        source.Case_1(param_1, param_2: default);
    }

    public static void Case_1(this Class_33 source)
    {
        source.Case_1(default(Class_33_Type_1?), default(Class_33_Type_2));
    }

    public static void Case_1(this Class_33 source, Class_33_Type_3 param_3)
    {
        source.Case_1(param_3, param_4: default);
    }
}