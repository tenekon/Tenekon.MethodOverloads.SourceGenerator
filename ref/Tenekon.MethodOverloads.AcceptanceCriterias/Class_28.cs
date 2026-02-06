using Tenekon.MethodOverloads.SourceGenerator;

namespace Tenekon.MethodOverloads.AcceptanceCriterias;

internal interface Class_28_Matcher
{
    [GenerateOverloads(Begin = nameof(param_1))]
    void Matcher_1(int param_1);
}

/// <summary>
/// Matcher with Begin only: only the matcher parameters are eligible to become optional (here, just param_1).
/// </summary>
[GenerateMethodOverloads(Matchers = [typeof(Class_28_Matcher)])]
public abstract class Class_28
{
    public abstract void Case_1(int param_1, string? param_2, bool param_3);
}

public static class Class_28_AcceptanceCriterias
{
    public static void Case_1(this Class_28 source, string? param_2, bool param_3)
    {
        source.Case_1(param_1: default, param_2, param_3);
    }
}