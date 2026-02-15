using System.Diagnostics.CodeAnalysis;

namespace Tenekon.MethodOverloads.AcceptanceCriterias.SupplyParameterType;

[SuppressMessage("MethodOverloadsGenerator", "MOG014")]
public sealed class SupplyParameterType_Class_7
{
    [GenerateOverloads(nameof(optionalObject))]
    [SupplyParameterType("TConstraint", typeof(SupplyParameterType_Constraint))]
    public void Case_1<TConstraint>(
        SupplyParameterType_IService<TConstraint>? constrainedService,
        object? optionalObject)
    {
    }
}

public static class SupplyParameterType_Class_7_AcceptanceCriterias
{
    public static void Case_1<TConstraint>(
        this SupplyParameterType_Class_7 source,
        SupplyParameterType_IService<TConstraint>? constrainedService)
    {
        source.Case_1<TConstraint>(constrainedService, default);
    }
}
