using System.Diagnostics.CodeAnalysis;
using Tenekon.MethodOverloads.SourceGenerator;

namespace Tenekon.MethodOverloads.AcceptanceCriterias;

internal class Class_16_Matcher
{
    [GenerateOverloads(Begin = nameof(param_2), End = nameof(param_2))]
    internal static extern void Matcher_1(int param_1, string? param_2);
}

/// <summary>
/// Method has both matcher-based and direct GenerateOverloads; expect no duplicates.
/// </summary>
[GenerateMethodOverloads(Matchers = [typeof(Class_16_Matcher)])]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public abstract class Class_16
{
    [GenerateOverloads(Begin = nameof(param_2), End = nameof(param_2))]
    public abstract void Case_1(int param_1, string? param_2);
}

/// <summary>
/// Expected extension overloads for Class_16 (no duplication).
/// </summary>
[SuppressMessage("ReSharper", "PreferConcreteValueOverDefault")]
public static class Class_16_AcceptanceCriterias
{
    public static void Case_1(this Class_16 source, int param_1) =>
        source.Case_1(param_1, param_2: null);
}



