using Tenekon.MethodOverloads.SourceGenerator;

namespace Tenekon.MethodOverloads.AcceptanceCriterias;

public readonly struct Class_40_Case_1_Param_1;

public readonly struct Class_40_Case_1_Param_2;

public readonly struct Class_40_Case_2_Param_1;

public readonly struct Class_40_Case_2_Param_2;

/// <summary>
/// OverloadVisibility precedence between target and matcher frames.
/// </summary>
[OverloadGenerationOptions(OverloadVisibility = OverloadVisibility.Internal)]
internal interface Class_40_Matcher
{
    [GenerateOverloads(nameof(param_a))]
    [OverloadGenerationOptions(OverloadVisibility = OverloadVisibility.Public)]
    void Matcher_1(Class_40_Case_1_Param_1 param_a);

    [GenerateOverloads(nameof(param_a))]
    void Matcher_2(Class_40_Case_2_Param_1 param_a);
}

[GenerateMethodOverloads(Matchers = [typeof(Class_40_Matcher)])]
public abstract class Class_40_1
{
    protected internal abstract void Case_1_1(Class_40_Case_1_Param_1 param_1, Class_40_Case_1_Param_2 param_2);

    [OverloadGenerationOptions(OverloadVisibility = OverloadVisibility.Internal)]
    protected internal abstract void Case_1_2(Class_40_Case_1_Param_1 param_1, Class_40_Case_1_Param_2 param_2);

    protected internal abstract void Case_2(Class_40_Case_2_Param_1 param_1, Class_40_Case_2_Param_2 param_2);
}

[GenerateMethodOverloads(Matchers = [typeof(Class_40_Matcher)])]
[OverloadGenerationOptions(OverloadVisibility = OverloadVisibility.Public)]
public abstract class Class_40_2
{
    [OverloadGenerationOptions(OverloadVisibility = OverloadVisibility.Internal)]
    protected internal abstract void Case_1_1(Class_40_Case_1_Param_1 param_1, Class_40_Case_1_Param_2 param_2);

    protected internal abstract void Case_1_2(Class_40_Case_1_Param_1 param_1, Class_40_Case_1_Param_2 param_2);    
}

/// <summary>
/// Expected extension overloads (or none) for Class_40 cases.
/// </summary>
public static class Class_40_AcceptanceCriterias
{
    public static void Case_1_1(this Class_40_1 source, Class_40_Case_1_Param_2 param_2)
    {
        source.Case_1_1(param_1: default, param_2);
    }

    internal static void Case_1_2(this Class_40_1 source, Class_40_Case_1_Param_2 param_2)
    {
        source.Case_1_2(param_1: default, param_2);
    }

    internal static void Case_2(this Class_40_1 source, Class_40_Case_2_Param_2 param_2)
    {
        source.Case_2(param_1: default, param_2);
    }

    internal static void Case_1_1(this Class_40_2 source, Class_40_Case_1_Param_2 param_2)
    {
        source.Case_1_1(param_1: default, param_2);
    }

    public static void Case_1_2(this Class_40_2 source, Class_40_Case_1_Param_2 param_2)
    {
        source.Case_1_2(param_1: default, param_2);
    }
}
