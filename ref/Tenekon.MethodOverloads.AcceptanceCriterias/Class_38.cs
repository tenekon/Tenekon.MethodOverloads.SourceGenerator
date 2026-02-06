using Tenekon.MethodOverloads.SourceGenerator;

namespace Tenekon.MethodOverloads.AcceptanceCriterias;

internal interface Class_38_MatcherA
{
    [GenerateOverloads(nameof(param_2))]
    void Matcher_1(int param_1, string param_2);
}

internal interface Class_38_MatcherB
{
    [GenerateOverloads(nameof(param_3))]
    void Matcher_1(int param_1, string param_2, bool param_3);
}

/// <summary>
/// Multiple GenerateMethodOverloads attributes on the same type.
/// Matchers from all attributes are unioned.
/// </summary>
[GenerateMethodOverloads(Matchers = [typeof(Class_38_MatcherA)])]
[GenerateMethodOverloads(Matchers = [typeof(Class_38_MatcherB)])]
public abstract class Class_38
{
    public abstract void Case_1(int param_1, string param_2, bool param_3);
}

public static class Class_38_AcceptanceCriterias
{
    public static void Case_1(this Class_38 source, int param_1, bool param_3)
    {
        source.Case_1(param_1, default(string), param_3);
    }

    public static void Case_1(this Class_38 source, int param_1, string param_2)
    {
        source.Case_1(param_1, param_2, default(bool));
    }
}