using System.Diagnostics.CodeAnalysis;

namespace Tenekon.MethodOverloads.AcceptanceCriterias.SupplyParameterType;

[SuppressMessage("MethodOverloadsGenerator", "MOG016")]
public sealed class SupplyParameterType_Class_6
{
    [GenerateOverloads(nameof(optionalObject))]
    [SupplyParameterType(nameof(TConstraint), typeof(SupplyParameterType_Constraint))]
    [SupplyParameterType(nameof(TConstraint), typeof(SupplyParameterType_AnotherConstraint))]
    public void Case_1<TConstraint>(
        SupplyParameterType_IService<TConstraint>? constrainedService,
        object? optionalObject)
    {
    }
}
