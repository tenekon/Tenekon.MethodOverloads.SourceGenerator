using System.Diagnostics.CodeAnalysis;

namespace Tenekon.MethodOverloads.AcceptanceCriterias.SupplyParameterType;

[SuppressMessage("MethodOverloadsGenerator", "MOG015")]
public sealed class SupplyParameterType_Class_8
{
    [GenerateOverloads(nameof(optionalObject))]
    [SupplyParameterType(nameof(SupplyParameterType_Constraint), typeof(SupplyParameterType_Constraint))]
    public void Case_1<TConstraint>(
        SupplyParameterType_IService<TConstraint>? constrainedService,
        object? optionalObject)
    {
    }
}

public static class SupplyParameterType_Class_8_AcceptanceCriterias
{
    public static void Case_1<TConstraint>(
        this SupplyParameterType_Class_8 source,
        SupplyParameterType_IService<TConstraint>? constrainedService)
    {
        source.Case_1<TConstraint>(constrainedService, default);
    }
}
