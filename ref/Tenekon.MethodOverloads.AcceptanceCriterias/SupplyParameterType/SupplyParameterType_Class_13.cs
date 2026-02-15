namespace Tenekon.MethodOverloads.AcceptanceCriterias.SupplyParameterType;

public sealed class SupplyParameterType_Class_13_Constraint_1;
public sealed class SupplyParameterType_Class_13_Constraint_2;

public sealed class SupplyParameterType_Class_13_Param<TConstraint>;

public interface SupplyParameterType_Class_13_Matcher
{
    [GenerateOverloads(Begin = nameof(optionalObject))]
    [SupplyParameterType(nameof(TConstraint), typeof(SupplyParameterType_Class_13_Constraint_1))]
    void Matcher_1<TConstraint>(
        SupplyParameterType_Class_13_Param<TConstraint> param,
        object? optionalObject);

    [GenerateOverloads(Begin = nameof(optionalObject))]
    [SupplyParameterType(nameof(TConstraint), typeof(SupplyParameterType_Class_13_Constraint_2))]
    void Matcher_2<TConstraint>(
        SupplyParameterType_Class_13_Param<TConstraint> param,
        object? optionalObject);
}

[GenerateMethodOverloads(Matchers = [typeof(SupplyParameterType_Class_13_Matcher)])]
public sealed class SupplyParameterType_Class_13
{
    public void Case_1<TConstraint>(
        SupplyParameterType_Class_13_Param<TConstraint> param,
        object? optionalObject)
    {
    }
}

public static class SupplyParameterType_Class_13_AcceptanceCriterias
{
    public static void Case_1(
        this SupplyParameterType_Class_13 source,
        SupplyParameterType_Class_13_Param<SupplyParameterType_Class_13_Constraint_1> param)
    {
        source.Case_1(param, default);
    }

    public static void Case_1(
        this SupplyParameterType_Class_13 source,
        SupplyParameterType_Class_13_Param<SupplyParameterType_Class_13_Constraint_2> param)
    {
        source.Case_1(param, default);
    }
}
