namespace Tenekon.MethodOverloads.AcceptanceCriterias.SupplyParameterType;

public sealed class SupplyParameterType_Class_9<TOuter>
{
    [GenerateOverloads(nameof(optionalObject))]
    [SupplyParameterType(nameof(TInner), typeof(SupplyParameterType_Constraint))]
    public void Case_1<TInner>(
        SupplyParameterType_IService<TInner>? constrainedService,
        object? optionalObject)
    {
    }
}

public static class SupplyParameterType_Class_9_AcceptanceCriterias
{
    public static void Case_1<TOuter>(
        this SupplyParameterType_Class_9<TOuter> source,
        SupplyParameterType_IService<SupplyParameterType_Constraint>? constrainedService)
    {
        source.Case_1<SupplyParameterType_Constraint>(constrainedService, default);
    }
}
