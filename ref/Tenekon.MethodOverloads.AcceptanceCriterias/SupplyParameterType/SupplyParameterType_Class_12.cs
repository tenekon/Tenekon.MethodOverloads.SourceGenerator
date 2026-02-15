namespace Tenekon.MethodOverloads.AcceptanceCriterias.SupplyParameterType;

public sealed class SupplyParameterType_Class_12_Constraint_1;
public sealed class SupplyParameterType_Class_12_Constraint_2;

public sealed class SupplyParameterType_Class_12_Param<TConstraint>;

public interface SupplyParameterType_Class_12_Matcher_1
{
    [GenerateOverloads(nameof(optionalObject))]
    [SupplyParameterType(nameof(TConstraint), typeof(SupplyParameterType_Class_12_Constraint_1))]
    void Case_1<TConstraint>(
        SupplyParameterType_Class_12_Param<TConstraint> param,
        object? optionalObject);
}

public interface SupplyParameterType_Class_12_Matcher_2
{
    [GenerateOverloads(nameof(optionalObject))]
    [SupplyParameterType(nameof(TConstraint), typeof(SupplyParameterType_Class_12_Constraint_2))]
    void Case_1<TConstraint>(
        SupplyParameterType_Class_12_Param<TConstraint> param,
        object? optionalObject);
}

[GenerateMethodOverloads(Matchers =
[
    typeof(SupplyParameterType_Class_12_Matcher_1),
    typeof(SupplyParameterType_Class_12_Matcher_2)
])]
public sealed class SupplyParameterType_Class_12
{
    public void Case_1<TConstraint>(
        SupplyParameterType_Class_12_Param<TConstraint> param,
        object? optionalObject)
    {
    }
}

public static class SupplyParameterType_Class_12_AcceptanceCriterias
{
    public static void Case_1(
        this SupplyParameterType_Class_12 source,
        SupplyParameterType_Class_12_Param<SupplyParameterType_Class_12_Constraint_1> param)
    {
        source.Case_1(param, default);
    }

    public static void Case_1(
        this SupplyParameterType_Class_12 source,
        SupplyParameterType_Class_12_Param<SupplyParameterType_Class_12_Constraint_2> param)
    {
        source.Case_1(param, default);
    }
}
