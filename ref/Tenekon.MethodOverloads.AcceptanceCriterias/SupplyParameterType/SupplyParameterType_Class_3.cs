using System.Collections.Generic;

namespace Tenekon.MethodOverloads.AcceptanceCriterias.SupplyParameterType;

public sealed class SupplyParameterType_Class_3
{
    [GenerateOverloads(nameof(optionalObject))]
    [SupplyParameterType(nameof(TItem), typeof(SupplyParameterType_Constraint))]
    public Dictionary<string, TItem> Case_1<TItem>(
        SupplyParameterType_IService<List<TItem>>? service,
        object? optionalObject)
    {
        return new Dictionary<string, TItem>();
    }
}

public static class SupplyParameterType_Class_3_AcceptanceCriterias
{
    public static Dictionary<string, SupplyParameterType_Constraint> Case_1(
        this SupplyParameterType_Class_3 source,
        SupplyParameterType_IService<List<SupplyParameterType_Constraint>>? service)
    {
        return source.Case_1<SupplyParameterType_Constraint>(service, default);
    }
}
