namespace Tenekon.MethodOverloads.AcceptanceCriterias.SupplyParameterType;

public sealed class SupplyParameterType_Class_14_Group_1;
public sealed class SupplyParameterType_Class_14_Group_2;

public sealed class SupplyParameterType_Class_14_Constraint_1;
public sealed class SupplyParameterType_Class_14_Constraint_2;

public sealed class SupplyParameterType_Class_14_Param<TConstraint>;

public sealed class SupplyParameterType_Class_14
{
    [GenerateOverloads(nameof(optionalObject))]
    [SupplyParameterType(
        nameof(TConstraint),
        typeof(SupplyParameterType_Class_14_Constraint_1),
        Group = typeof(SupplyParameterType_Class_14_Group_1))]
    [SupplyParameterType(
        nameof(TConstraint),
        typeof(SupplyParameterType_Class_14_Constraint_2),
        Group = typeof(SupplyParameterType_Class_14_Group_2))]
    public void Case_1<TConstraint>(
        SupplyParameterType_Class_14_Param<TConstraint> param,
        object? optionalObject)
    {
    }
}

public static class SupplyParameterType_Class_14_AcceptanceCriterias
{
    public static void Case_1(
        this SupplyParameterType_Class_14 source,
        SupplyParameterType_Class_14_Param<SupplyParameterType_Class_14_Constraint_1> param)
    {
        source.Case_1<SupplyParameterType_Class_14_Constraint_1>(param, default);
    }

    public static void Case_1(
        this SupplyParameterType_Class_14 source,
        SupplyParameterType_Class_14_Param<SupplyParameterType_Class_14_Constraint_2> param)
    {
        source.Case_1<SupplyParameterType_Class_14_Constraint_2>(param, default);
    }
}
