namespace Tenekon.MethodOverloads.AcceptanceCriterias.SupplyParameterType;

public sealed class SupplyParameterType_Class_11_Constraint_1;
public sealed class SupplyParameterType_Class_11_Constraint_2;
public sealed class SupplyParameterType_Class_11_Constraint_3;
public sealed class SupplyParameterType_Class_11_Constraint_4;

public sealed class SupplyParameterType_Class_11_Case_1_Param_1<TConstraint>;
public sealed class SupplyParameterType_Class_11_Case_2_Param_1<TConstraint>;

[SupplyParameterType(nameof(TConstraint), typeof(SupplyParameterType_Class_11_Constraint_4))]
public interface SupplyParameterType_Class_11_Matcher<TConstraint>
{
    [GenerateOverloads(nameof(param_2))]
    [SupplyParameterType(nameof(TConstraint), typeof(SupplyParameterType_Class_11_Constraint_3))]
    void Case_1(
        SupplyParameterType_Class_11_Case_1_Param_1<TConstraint> param_1,
        object? param_2);
    
    [GenerateOverloads(nameof(param_2))]
    void Case_2(
        SupplyParameterType_Class_11_Case_2_Param_1<TConstraint> param_1,
        object? param_2);
}

[GenerateMethodOverloads(Matchers = [typeof(SupplyParameterType_Class_11_Matcher<>)])]
[SupplyParameterType(nameof(TConstraint), typeof(SupplyParameterType_Class_11_Constraint_2))]
public sealed class SupplyParameterType_Class_11_1<TConstraint>
{
    [SupplyParameterType(nameof(TConstraint), typeof(SupplyParameterType_Class_11_Constraint_1))]
    public void Case_1(
        SupplyParameterType_Class_11_Case_1_Param_1<TConstraint> param_1,
        object? param_2)
    {
    }
    
    public void Case_2(
        SupplyParameterType_Class_11_Case_2_Param_1<TConstraint> param_1,
        object? param_2)
    {
    }
}

public static class SupplyParameterType_Class_11_1_AcceptanceCriterias
{
    public static void Case_1(
        this SupplyParameterType_Class_11_1<SupplyParameterType_Class_11_Constraint_1> source,
        SupplyParameterType_Class_11_Case_1_Param_1<SupplyParameterType_Class_11_Constraint_1> param_1)
    {
        source.Case_1(param_1, default);
    }
    
    public static void Case_2(
        this SupplyParameterType_Class_11_1<SupplyParameterType_Class_11_Constraint_2> source,
        SupplyParameterType_Class_11_Case_2_Param_1<SupplyParameterType_Class_11_Constraint_2> param_1)
    {
        source.Case_2(param_1, default);
    }
}

[GenerateMethodOverloads(Matchers = [typeof(SupplyParameterType_Class_11_Matcher<>)])]
public sealed class SupplyParameterType_Class_11_2<TConstraint>
{
    [SupplyParameterType(nameof(TConstraint), typeof(SupplyParameterType_Class_11_Constraint_1))]
    public void Case_1(
        SupplyParameterType_Class_11_Case_1_Param_1<TConstraint> param_1,
        object? param_2)
    {
    }
    
    public void Case_2(
        SupplyParameterType_Class_11_Case_2_Param_1<TConstraint> param_1,
        object? param_2)
    {
    }
}

public static class SupplyParameterType_Class_11_2_AcceptanceCriterias
{
    public static void Case_1(
        this SupplyParameterType_Class_11_2<SupplyParameterType_Class_11_Constraint_1> source,
        SupplyParameterType_Class_11_Case_1_Param_1<SupplyParameterType_Class_11_Constraint_1> param_1)
    {
        source.Case_1(param_1, default);
    }
    
    public static void Case_2(
        this SupplyParameterType_Class_11_2<SupplyParameterType_Class_11_Constraint_4> source,
        SupplyParameterType_Class_11_Case_2_Param_1<SupplyParameterType_Class_11_Constraint_4> param_1)
    {
        source.Case_2(param_1, default);
    }
}
