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
