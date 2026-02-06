using Tenekon.MethodOverloads.SourceGenerator;

namespace Tenekon.MethodOverloads.AcceptanceCriterias;

public readonly struct Class_39_Case_1_Param_1;

public readonly struct Class_39_Case_1_Param_2;

public readonly struct Class_39_Case_1_Param_3;

public readonly struct Class_39_Case_1_Param_4;

public readonly struct Class_39_Case_2_Param_1;

public readonly struct Class_39_Case_2_Param_2;

public readonly struct Class_39_Case_2_Param_3;

public readonly struct Class_39_Case_2_Param_4;

/// <summary>
/// SubsequenceStrategy precedence between target and matcher frames.
/// </summary>
[OverloadGenerationOptions(SubsequenceStrategy = OverloadSubsequenceStrategy.PrefixOnly)]
internal interface Class_39_Matcher
{
    [OverloadGenerationOptions(SubsequenceStrategy = OverloadSubsequenceStrategy.UniqueBySignature)]
    [GenerateOverloads(Begin = nameof(param_a), End = nameof(param_b))]
    void Matcher_1(Class_39_Case_1_Param_1 param_a, Class_39_Case_1_Param_2 param_b);

    [GenerateOverloads(Begin = nameof(param_a), End = nameof(param_b))]
    void Matcher_2(Class_39_Case_2_Param_1 param_a, Class_39_Case_2_Param_2 param_b);
}

[GenerateMethodOverloads(Matchers = [typeof(Class_39_Matcher)])]
public abstract class Class_39_1
{
    [OverloadGenerationOptions(SubsequenceStrategy = OverloadSubsequenceStrategy.PrefixOnly)]
    public abstract void Case_1_1(Class_39_Case_1_Param_1 param_1, Class_39_Case_1_Param_2 param_2);

    public abstract void Case_1_2(Class_39_Case_1_Param_1 param_1, Class_39_Case_1_Param_2 param_2);

    public abstract void Case_2(Class_39_Case_2_Param_1 param_1, Class_39_Case_2_Param_2 param_2);
}

[GenerateMethodOverloads(Matchers = [typeof(Class_39_Matcher)])]
[OverloadGenerationOptions(SubsequenceStrategy = OverloadSubsequenceStrategy.UniqueBySignature)]
public abstract class Class_39_2
{
    [OverloadGenerationOptions(SubsequenceStrategy = OverloadSubsequenceStrategy.PrefixOnly)]
    public abstract void Case_1_1(Class_39_Case_1_Param_1 param_1, Class_39_Case_1_Param_2 param_2);

    public abstract void Case_1_2(Class_39_Case_1_Param_1 param_1, Class_39_Case_1_Param_2 param_2);
}

/// <summary>
/// Expected extension overloads (or none) for Class_39 cases.
/// </summary>
public static class Class_39_AcceptanceCriterias
{
    public static void Case_1_1(this Class_39_1 source)
    {
        source.Case_1_1(param_1: default, param_2: default);
    }

    public static void Case_1_1(this Class_39_1 source, Class_39_Case_1_Param_1 param_1)
    {
        source.Case_1_1(param_1, param_2: default);
    }

    public static void Case_1_2(this Class_39_1 source)
    {
        source.Case_1_2(param_1: default, param_2: default);
    }

    public static void Case_1_2(this Class_39_1 source, Class_39_Case_1_Param_1 param_1)
    {
        source.Case_1_2(param_1, param_2: default);
    }

    public static void Case_1_2(this Class_39_1 source, Class_39_Case_1_Param_2 param_2)
    {
        source.Case_1_2(param_1: default, param_2);
    }

    public static void Case_2(this Class_39_1 source)
    {
        source.Case_2(param_1: default, param_2: default);
    }

    public static void Case_2(this Class_39_1 source, Class_39_Case_2_Param_1 param_1)
    {
        source.Case_2(param_1, param_2: default);
    }

    public static void Case_1_1(this Class_39_2 source)
    {
        source.Case_1_1(param_1: default, param_2: default);
    }

    public static void Case_1_1(this Class_39_2 source, Class_39_Case_1_Param_1 param_1)
    {
        source.Case_1_1(param_1, param_2: default);
    }

    public static void Case_1_2(this Class_39_2 source)
    {
        source.Case_1_2(param_1: default, param_2: default);
    }

    public static void Case_1_2(this Class_39_2 source, Class_39_Case_1_Param_1 param_1)
    {
        source.Case_1_2(param_1, param_2: default);
    }

    public static void Case_1_2(this Class_39_2 source, Class_39_Case_1_Param_2 param_2)
    {
        source.Case_1_2(param_1: default, param_2);
    }
}