using Tenekon.MethodOverloads.SourceGenerator;

namespace Tenekon.MethodOverloads.AcceptanceCriterias;

/// <summary>
/// Matcher type for mixed direct + matcher generation.
/// </summary>
internal interface Class_37_Matcher
{
    /// <summary>
    /// Optional window anchored at param_3.
    /// </summary>
    [GenerateOverloads(nameof(param_3))]
    void Matcher_1(int param_1, string param_2, bool param_3);
}

/// <summary>
/// Mixed direct window + matchers-only GenerateOverloads.
/// </summary>
public abstract class Class_37
{
    /// <summary>
    /// Target method with one direct window and one matcher set.
    /// </summary>
    [GenerateOverloads(nameof(param_2))]
    [GenerateOverloads(Matchers = [typeof(Class_37_Matcher)])]
    public abstract void Case_1(int param_1, string param_2, bool param_3);
}

public static class Class_37_AcceptanceCriterias
{
    public static void Case_1(this Class_37 source, int param_1, bool param_3)
    {
        source.Case_1(param_1, default(string), param_3);
    }

    public static void Case_1(this Class_37 source, int param_1, string param_2)
    {
        source.Case_1(param_1, param_2, default(bool));
    }
}