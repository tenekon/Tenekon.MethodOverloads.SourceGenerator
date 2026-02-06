using Tenekon.MethodOverloads.SourceGenerator;

namespace Tenekon.MethodOverloads.AcceptanceCriterias;

/// <summary>
/// Overloads should be generated for both base and derived methods when annotated.
/// </summary>
public class Class_22_Base
{
    [GenerateOverloads(nameof(param_2))]
    public virtual void Case_1(int param_1, string? param_2) { }
}

public class Class_22_Derived : Class_22_Base
{
    [GenerateOverloads(nameof(param_2))]
    public override void Case_1(int param_1, string? param_2) { }
}

/// <summary>
/// Expected extension overloads for base and derived methods.
/// </summary>
public static class Class_22_AcceptanceCriterias
{
    public static void Case_1(this Class_22_Base source, int param_1)
    {
        source.Case_1(param_1, param_2: null);
    }

    public static void Case_1(this Class_22_Derived source, int param_1)
    {
        source.Case_1(param_1, param_2: null);
    }
}