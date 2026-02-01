using System.Diagnostics.CodeAnalysis;
using Tenekon.MethodOverloads.SourceGenerator;

namespace Tenekon.MethodOverloads.AcceptanceCriterias;

/// <summary>
/// Accessibility rules for overload generation.
/// Skip private; generate for public/internal/protected/protected internal.
/// </summary>
public class Class_15
{
    [GenerateOverloads(Begin = nameof(param_2), End = nameof(param_2))]
    public void Case_1(int param_1, string? param_2) { }

    [GenerateOverloads(Begin = nameof(param_2), End = nameof(param_2))]
    internal void Case_2(int param_1, string? param_2) { }

    [GenerateOverloads(Begin = nameof(param_2), End = nameof(param_2))]
    protected void Case_3(int param_1, string? param_2) { }

    [GenerateOverloads(Begin = nameof(param_2), End = nameof(param_2))]
    private void Case_4(int param_1, string? param_2) { }

    [GenerateOverloads(Begin = nameof(param_2), End = nameof(param_2))]
    protected internal void Case_5(int param_1, string? param_2) { }
}

/// <summary>
/// Overload visibility override via OverloadGenerationOptionsAttribute.
/// </summary>
public class Class_15_Visibility
{
    [OverloadGenerationOptions(OverloadVisibility = OverloadVisibility.Internal)]
    [GenerateOverloads(Begin = nameof(param_2), End = nameof(param_2))]
    public void Case_1(int param_1, string? param_2) { }

    [OverloadGenerationOptions(OverloadVisibility = OverloadVisibility.Public)]
    [GenerateOverloads(Begin = nameof(param_2), End = nameof(param_2))]
    internal void Case_2(int param_1, string? param_2) { }
}

/// <summary>
/// Expected extension overloads (or none) for accessibility scenarios.
/// </summary>
[SuppressMessage("ReSharper", "PreferConcreteValueOverDefault")]
public static class Class_15_AcceptanceCriterias
{
    public static void Case_1(this Class_15 source, int param_1) =>
        source.Case_1(param_1, param_2: null);

    internal static void Case_2(this Class_15 source, int param_1) =>
        source.Case_2(param_1, param_2: null);

    // No extension methods for Case_3

    // No extension methods for Case_4

    internal static void Case_5(this Class_15 source, int param_1) =>
        source.Case_5(param_1, param_2: null);

    internal static void Case_1(this Class_15_Visibility source, int param_1) =>
        source.Case_1(param_1, param_2: null);

    public static void Case_2(this Class_15_Visibility source, int param_1) =>
        source.Case_2(param_1, param_2: null);
}



