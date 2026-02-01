using System.Diagnostics.CodeAnalysis;
using Tenekon.MethodOverloads.SourceGenerator;

namespace Tenekon.MethodOverloads.AcceptanceCriterias;

/// <summary>
/// Ref/out/in parameters: omit only when legal; no defaults for ref/out/in.
/// </summary>
public abstract class Class_12
{
    [SuppressMessage("MethodOverloadsGenerator", "MOG005")]
    [GenerateOverloads(Begin = nameof(param_2), End = nameof(param_4))]
    public abstract void Case_1(int param_1, ref int param_2, out int param_3, in int param_4);

    [GenerateOverloads(Begin = nameof(param_3), End = nameof(param_3))]
    public abstract void Case_2(int param_1, ref int param_2, string? param_3, in int param_4);
}

/// <summary>
/// Expected extension overloads (or none) for ref/out/in scenarios.
/// </summary>
[SuppressMessage("ReSharper", "PreferConcreteValueOverDefault")]
public static class Class_12_AcceptanceCriterias
{
    // No extension methods for Case_1 (would require defaults for ref/out/in).

    public static void Case_2(this Class_12 source, int param_1, ref int param_2, in int param_4) =>
        source.Case_2(param_1, ref param_2, param_3: null, param_4);
}


