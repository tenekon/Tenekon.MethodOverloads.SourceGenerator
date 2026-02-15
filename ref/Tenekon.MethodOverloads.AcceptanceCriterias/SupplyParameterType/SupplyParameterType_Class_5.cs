namespace Tenekon.MethodOverloads.AcceptanceCriterias.SupplyParameterType;

public sealed class SupplyParameterType_Class_5
{
    [GenerateOverloads(nameof(optionalObject))]
    [SupplyParameterType(nameof(TConstraint), typeof(SupplyParameterType_Constraint))]
    [SupplyParameterType(nameof(TConstraint), typeof(SupplyParameterType_Constraint))]
    public void Case_1<TConstraint>(
        SupplyParameterType_IService<TConstraint>? constrainedService,
        object? optionalObject)
    {
    }
}

public static class SupplyParameterType_Class_5_AcceptanceCriterias
{
    public static void Case_1(
        this SupplyParameterType_Class_5 source,
        SupplyParameterType_IService<SupplyParameterType_Constraint>? constrainedService)
    {
        source.Case_1<SupplyParameterType_Constraint>(constrainedService, default);
    }
}
