namespace Tenekon.MethodOverloads.AcceptanceCriterias.SupplyParameterType;

public enum SupplyParameterType_Class_15_Group
{
    One = 1,
    Two = 2
}

public sealed class SupplyParameterType_Class_15_Constraint_1;
public sealed class SupplyParameterType_Class_15_Constraint_2;

public sealed class SupplyParameterType_Class_15_Param<TConstraint>;

public sealed class SupplyParameterType_Class_15
{
    [GenerateOverloads(nameof(optionalObject))]
    [SupplyParameterType(
        nameof(TConstraint),
        typeof(SupplyParameterType_Class_15_Constraint_1),
        Group = "GroupOne")]
    [SupplyParameterType(
        nameof(TConstraint),
        typeof(SupplyParameterType_Class_15_Constraint_2),
        Group = SupplyParameterType_Class_15_Group.Two)]
    public void Case_1<TConstraint>(
        SupplyParameterType_Class_15_Param<TConstraint> param,
        object? optionalObject)
    {
    }
}

public static class SupplyParameterType_Class_15_AcceptanceCriterias
{
    public static void Case_1(
        this SupplyParameterType_Class_15 source,
        SupplyParameterType_Class_15_Param<SupplyParameterType_Class_15_Constraint_1> param)
    {
        source.Case_1<SupplyParameterType_Class_15_Constraint_1>(param, default);
    }

    public static void Case_1(
        this SupplyParameterType_Class_15 source,
        SupplyParameterType_Class_15_Param<SupplyParameterType_Class_15_Constraint_2> param)
    {
        source.Case_1<SupplyParameterType_Class_15_Constraint_2>(param, default);
    }
}
