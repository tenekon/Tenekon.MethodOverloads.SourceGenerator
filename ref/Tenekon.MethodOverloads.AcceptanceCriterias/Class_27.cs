using Tenekon.MethodOverloads.SourceGenerator;

namespace Tenekon.MethodOverloads.AcceptanceCriterias;

internal interface Class_27_Matcher
{
    [GenerateOverloads(nameof(param_2))]
    void Matcher_1(int param_1, string? param_2, bool param_3);

    [GenerateOverloads(nameof(param_3))]
    void Matcher_2(int param_1, string? param_2, bool param_3);
}

/// <summary>
/// Multiple matcher methods apply to a single target method.
/// </summary>
[GenerateMethodOverloads(Matchers = [typeof(Class_27_Matcher)])]
public abstract class Class_27
{
    public abstract void Case_1(int param_1, string? param_2, bool param_3);
}

public static class Class_27_AcceptanceCriterias
{
    public static void Case_1(this Class_27 source, int param_1, bool param_3) =>
        source.Case_1(param_1, param_2: null, param_3);

    public static void Case_1(this Class_27 source, int param_1, string? param_2) =>
        source.Case_1(param_1, param_2, param_3: false);
}

