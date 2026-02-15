namespace Tenekon.MethodOverloads.AcceptanceCriterias.Bug;

public sealed class Bug_Class_1_Constraint_1;

public sealed class Bug_Class_1_Constraint_2;

public sealed class Bug_Class_1_ReturnType;

public interface Bug_Class_1_Type_1;

public interface Bug_Class_1_Type_2;

public sealed class Bug_Class_1_Type_3;

public interface Bug_Class_1_Type_4<TConstraint>;

internal interface Bug_Class_1_ICommandRuntimeFactoryMatchers
{
    [GenerateOverloads(Begin = nameof(param_3))]
    [OverloadGenerationOptions(BucketType = typeof(Bug_Class_1_Bucket))]
    [SupplyParameterType(
        nameof(TConstraint),
        typeof(Bug_Class_1_Constraint_1),
        Group = typeof(Bug_Class_1_Constraint_1))]
    [SupplyParameterType(
        nameof(TConstraint),
        typeof(Bug_Class_1_Constraint_2),
        Group = typeof(Bug_Class_1_Constraint_2))]
    void Matcher<TConstraint>(Bug_Class_1_Type_3? param_3, Bug_Class_1_Type_4<TConstraint>? param_4);
}

[GenerateMethodOverloads(Matchers = [typeof(Bug_Class_1_ICommandRuntimeFactoryMatchers)])]
public interface Bug_Class_1<TConstraint>
{
    Bug_Class_1_ReturnType Create(
        Bug_Class_1_Type_1 param_1,
        Bug_Class_1_Type_2 param_2,
        Bug_Class_1_Type_3? param_3,
        Bug_Class_1_Type_4<TConstraint>? param_4);
}

[GenerateMethodOverloads(Matchers = [typeof(Bug_Class_1_ICommandRuntimeFactoryMatchers)])]
public static partial class Bug_Class_1_Bucket
{
}

public static class Bug_Class_1_AcceptanceCriterias
{
    public static Bug_Class_1_ReturnType Create(
        this Bug_Class_1<Bug_Class_1_Constraint_1> source,
        Bug_Class_1_Type_1 param_1,
        Bug_Class_1_Type_2 param_2,
        Bug_Class_1_Type_3? param_3)
    {
        return source.Create(param_1, param_2, param_3, default);
    }

    public static Bug_Class_1_ReturnType Create(
        this Bug_Class_1<Bug_Class_1_Constraint_1> source,
        Bug_Class_1_Type_1 param_1,
        Bug_Class_1_Type_2 param_2,
        Bug_Class_1_Type_4<Bug_Class_1_Constraint_1>? modelRegistry)
    {
        return source.Create(param_1, param_2, default, modelRegistry);
    }

    public static Bug_Class_1_ReturnType Create(
        this Bug_Class_1<Bug_Class_1_Constraint_1> source,
        Bug_Class_1_Type_1 param_1,
        Bug_Class_1_Type_2 param_2)
    {
        return source.Create(param_1, param_2, default, default);
    }

    public static Bug_Class_1_ReturnType Create(
        this Bug_Class_1<Bug_Class_1_Constraint_2> source,
        Bug_Class_1_Type_1 param_1,
        Bug_Class_1_Type_2 param_2,
        Bug_Class_1_Type_3? param_3)
    {
        return source.Create(param_1, param_2, param_3, default);
    }

    public static Bug_Class_1_ReturnType Create(
        this Bug_Class_1<Bug_Class_1_Constraint_2> source,
        Bug_Class_1_Type_1 param_1,
        Bug_Class_1_Type_2 param_2,
        Bug_Class_1_Type_4<Bug_Class_1_Constraint_2>? param_4)
    {
        return source.Create(param_1, param_2, default, param_4);
    }

    public static Bug_Class_1_ReturnType Create(
        this Bug_Class_1<Bug_Class_1_Constraint_2> source,
        Bug_Class_1_Type_1 param_1,
        Bug_Class_1_Type_2 param_2)
    {
        return source.Create(param_1, param_2, default, default);
    }
}