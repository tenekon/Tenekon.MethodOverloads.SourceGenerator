using Tenekon.MethodOverloads.SourceGenerator;

namespace Tenekon.MethodOverloads.AcceptanceCriterias;

/// <summary>
/// Multiple direct GenerateOverloads attributes on a single method.
/// </summary>
public abstract class Class_35
{
    /// <summary>
    /// Target method with two direct windows (param_2 and param_3).
    /// </summary>
    [GenerateOverloads(nameof(param_2))]
    [GenerateOverloads(nameof(param_3))]
    public abstract void Case_1(int param_1, string param_2, bool param_3);
}

/// <summary>
/// Expected overloads for the direct-window case.
/// </summary>
public static class Class_35_AcceptanceCriterias
{
    public static void Case_1(this Class_35 source, int param_1, bool param_3)
    {
        source.Case_1(param_1, default(string), param_3);
    }

    public static void Case_1(this Class_35 source, int param_1, string param_2)
    {
        source.Case_1(param_1, param_2, default(bool));
    }
}