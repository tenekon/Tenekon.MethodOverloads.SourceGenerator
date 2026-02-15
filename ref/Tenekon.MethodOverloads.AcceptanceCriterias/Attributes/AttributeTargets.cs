#pragma warning disable IDE0051

namespace Tenekon.MethodOverloads.AcceptanceCriterias.Attributes;

[GenerateMethodOverloads]
public class GenerateMethodOverloadsOnClass
{
}

[GenerateMethodOverloads]
public struct GenerateMethodOverloadsOnStruct
{
}

[GenerateMethodOverloads]
public interface GenerateMethodOverloadsOnInterface
{
}

public class GenerateOverloadsTargets
{
    [GenerateOverloads]
    public void GenerateOverloadsOnMethod(int param_1)
    {
    }
}

public static class GenerateOverloadsTargets_AcceptanceCriterias
{
    public static void GenerateOverloadsOnMethod(this GenerateOverloadsTargets source)
    {
        source.GenerateOverloadsOnMethod(param_1: default);
    }
}

[OverloadGenerationOptions]
public class OverloadGenerationOptionsOnClass
{
}

[OverloadGenerationOptions]
public struct OverloadGenerationOptionsOnStruct
{
}

[OverloadGenerationOptions]
public interface OverloadGenerationOptionsOnInterface
{
}

public class OverloadGenerationOptionsTargets
{
    [OverloadGenerationOptions]
    public void OverloadGenerationOptionsOnMethod(int param_1)
    {
    }
}

[MatcherUsage(nameof(GenerateOverloadsTargets.GenerateOverloadsOnMethod))]
public class MatcherUsageTargets
{
}

public class SupplyParameterTypeTargets
{
    [SupplyParameterType(nameof(TValue), typeof(object))]
    public void SupplyParameterTypeOnMethod<TValue>(TValue value)
    {
    }
}
#pragma warning restore IDE0051
