using Tenekon.MethodOverloads.SourceGenerator;

namespace Tenekon.MethodOverloads.AcceptanceCriterias;

/// <summary>
/// Operators and constructors should not be considered for overload generation.
/// </summary>
public class Class_23
{
    public Class_23(int value) { }

    public static Class_23 operator +(Class_23 left, Class_23 right) => left;

    [GenerateOverloads(Begin = nameof(param_2), End = nameof(param_2))]
    public void Case_1(int param_1, string? param_2) { }
}

/// <summary>
/// Expected extension overloads for Class_23.
/// </summary>
public static class Class_23_AcceptanceCriterias
{
    public static void Case_1(this Class_23 source, int param_1) =>
        source.Case_1(param_1, param_2: null);
}
