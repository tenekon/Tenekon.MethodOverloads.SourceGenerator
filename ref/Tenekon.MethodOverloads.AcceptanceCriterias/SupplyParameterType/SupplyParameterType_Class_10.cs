namespace Tenekon.MethodOverloads.AcceptanceCriterias.SupplyParameterType;

public interface SupplyParameterType_Class_10_Matcher
{
    [GenerateOverloads(nameof(optionalObject))]
    [SupplyParameterType(nameof(TConstraint), typeof(SupplyParameterType_Constraint))]
    void Case_1<TConstraint>(
        SupplyParameterType_IService<TConstraint>? constrainedService,
        object? optionalObject);
}

[GenerateMethodOverloads(Matchers = [typeof(SupplyParameterType_Class_10_Matcher)])]
public sealed class SupplyParameterType_Class_10
{
    public void Case_1<TConstraint>(
        SupplyParameterType_IService<TConstraint>? constrainedService,
        object? optionalObject)
    {
    }
}

public static class SupplyParameterType_Class_10_AcceptanceCriterias
{
    public static void Case_1(
        this SupplyParameterType_Class_10 source,
        SupplyParameterType_IService<SupplyParameterType_Constraint>? constrainedService)
    {
        source.Case_1(constrainedService, default);
    }
}
