namespace Tenekon.MethodOverloads.AcceptanceCriterias;

/// <summary>
/// Generic methods with constraints; expect overloads for all unique subsequences.
/// </summary>
public abstract class Class_13
{
    [GenerateOverloads(Begin = nameof(param_2), End = nameof(param_3))]
    public abstract void Case_1<T>(int param_1, T? param_2, bool param_3) where T : class, new();
}

/// <summary>
/// Expected extension overloads for generic method cases.
/// </summary>
public static class Class_13_AcceptanceCriterias
{
    public static void Case_1<T>(this Class_13 source, int param_1, T? param_2) where T : class, new()
    {
        source.Case_1(param_1, param_2, param_3: false);
    }

    public static void Case_1<T>(this Class_13 source, int param_1, bool param_3) where T : class, new()
    {
        source.Case_1<T>(param_1, param_2: null, param_3);
    }

    public static void Case_1<T>(this Class_13 source, int param_1) where T : class, new()
    {
        source.Case_1<T>(param_1, param_2: null, param_3: false);
    }
}