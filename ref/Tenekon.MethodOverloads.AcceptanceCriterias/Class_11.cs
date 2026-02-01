using System.Diagnostics.CodeAnalysis;
using Tenekon.MethodOverloads.SourceGenerator;

namespace Tenekon.MethodOverloads.AcceptanceCriterias;

/// <summary>
/// Params array handling when included/excluded by the optional window.
/// </summary>
public abstract class Class_11
{
    // Optional window includes the params parameter.
    [GenerateOverloads(Begin = nameof(param_2), End = nameof(param_2))]
    public abstract void Case_1(int param_1, params string[] param_2);

    // Optional window excludes the params parameter.
    [SuppressMessage("MethodOverloadsGenerator", "MOG004")]
    [GenerateOverloads(Begin = nameof(param_1), End = nameof(param_1))]
    public abstract void Case_2(int param_1, params string[] param_2);
}

/// <summary>
/// Expected extension overloads (or none) for params-array scenarios.
/// </summary>
[SuppressMessage("ReSharper", "PreferConcreteValueOverDefault")]
public static class Class_11_AcceptanceCriterias
{
    public static void Case_1(this Class_11 source, int param_1) =>
        source.Case_1(param_1, param_2: []);

    // No extension methods for Case_2
}


