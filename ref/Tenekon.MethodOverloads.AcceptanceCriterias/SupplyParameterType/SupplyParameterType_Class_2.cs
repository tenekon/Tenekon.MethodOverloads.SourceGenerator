namespace Tenekon.MethodOverloads.AcceptanceCriterias.SupplyParameterType;

public sealed class SupplyParameterType_Class_2
{
    [GenerateOverloads(nameof(optionalObject))]
    [SupplyParameterType(nameof(TLeft), typeof(SupplyParameterType_Constraint))]
    public void Case_1<TLeft, TRight>(
        SupplyParameterType_IService<TLeft>? left,
        SupplyParameterType_IService<TRight>? right,
        object? optionalObject)
    {
    }
}

public static class SupplyParameterType_Class_2_AcceptanceCriterias
{
    public static void Case_1<TRight>(
        this SupplyParameterType_Class_2 source,
        SupplyParameterType_IService<SupplyParameterType_Constraint>? left,
        SupplyParameterType_IService<TRight>? right)
    {
        source.Case_1<SupplyParameterType_Constraint, TRight>(left, right, default);
    }
}
