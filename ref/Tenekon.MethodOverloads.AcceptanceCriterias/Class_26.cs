using Tenekon.MethodOverloads.SourceGenerator;

namespace Tenekon.MethodOverloads.AcceptanceCriterias;

internal interface Class_26_Matcher
{
    [GenerateOverloads(nameof(param_2))]
    void Matcher_1(int param_1, string? param_2);

    [GenerateOverloads(nameof(param_2))]
    void Matcher_2(int param_1, string param_2, bool param_3);
}

/// <summary>
/// Nullability-aware matching: string? and string are treated as different types.
/// </summary>
[GenerateMethodOverloads(Matchers = [typeof(Class_26_Matcher)])]
public abstract class Class_26
{
    public abstract void Case_1(int param_1, string? param_2, bool param_3);

    public abstract void Case_2(int param_1, string param_2, bool param_3);
}

public static class Class_26_AcceptanceCriterias
{
    public static void Case_1(this Class_26 source, int param_1, bool param_3)
    {
        source.Case_1(param_1, param_2: null, param_3);
    }

    public static void Case_2(this Class_26 source, int param_1, bool param_3)
    {
        source.Case_2(param_1, default(string), param_3);
    }
}