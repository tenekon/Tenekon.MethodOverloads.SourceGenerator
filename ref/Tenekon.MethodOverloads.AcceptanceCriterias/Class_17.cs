using System.Diagnostics.CodeAnalysis;
using Tenekon.MethodOverloads.SourceGenerator;

namespace Tenekon.MethodOverloads.AcceptanceCriterias;

[SuppressMessage("ReSharper", "PartialTypeWithSinglePart")]
public abstract partial class Class_17
{
    public abstract void Case_2(int param_1);
}

/// <summary>
/// Existing overload already matches a would-be generated signature.
/// Generator must not emit duplicates.
/// </summary>
[SuppressMessage("ReSharper", "PartialTypeWithSinglePart")]
public abstract partial class Class_17
{
    [SuppressMessage("MethodOverloadsGenerator", "MOG006")]
    [GenerateOverloads(Begin = nameof(param_2), End = nameof(param_2))]
    public abstract void Case_1(int param_1, string? param_2);

    public abstract void Case_1(int param_1);

    [SuppressMessage("MethodOverloadsGenerator", "MOG006")]
    [GenerateOverloads(Begin = nameof(param_2), End = nameof(param_2))]
    public abstract void Case_2(int param_1, string? param_2);

    [GenerateOverloads(Begin = nameof(param_2), End = nameof(param_2))]
    public abstract void Case_3(int param_1, string? param_2);
}

/// <summary>
/// Expected extension overloads (none due to collision with existing overload).
/// </summary>
[SuppressMessage("ReSharper", "PreferConcreteValueOverDefault")]
public static class Class_17_AcceptanceCriterias
{
    // No extension methods for Case_1

    // No extension methods for Case_2

    public static void Case_3(this Class_17 source, int param_1) => source.Case_3(param_1, param_2: null);
}

