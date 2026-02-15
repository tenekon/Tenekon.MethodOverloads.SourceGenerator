namespace Tenekon.MethodOverloads.AcceptanceCriterias.SupplyParameterType;

public sealed class SupplyParameterType_Class_4
{
    [GenerateOverloads(nameof(optionalObject))]
    [SupplyParameterType(nameof(TLeft), typeof(SupplyParameterType_Constraint))]
    [SupplyParameterType(nameof(TRight), typeof(SupplyParameterType_AnotherConstraint))]
    public void Case_1<TLeft, TRight>(
        SupplyParameterType_IService<TLeft>? left,
        SupplyParameterType_IService<TRight>? right,
        object? optionalObject)
    {
    }
}

public static class SupplyParameterType_Class_4_AcceptanceCriterias
{
    public static void Case_1(
        this SupplyParameterType_Class_4 source,
        SupplyParameterType_IService<SupplyParameterType_Constraint>? left,
        SupplyParameterType_IService<SupplyParameterType_AnotherConstraint>? right)
    {
        source.Case_1<SupplyParameterType_Constraint, SupplyParameterType_AnotherConstraint>(left, right, default);
    }
}
