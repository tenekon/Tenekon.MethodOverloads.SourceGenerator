using Tenekon.MethodOverloads.SourceGenerator;

namespace Tenekon.MethodOverloads.AcceptanceCriterias;

internal static class Class_24_Matcher
{
    [GenerateOverloads(nameof(param_2))]
    internal static extern void Matcher_1(int param_1, string? param_2);
}

/// <summary>
/// Type-level matcher applies to class/struct/interface targets.
/// </summary>
[GenerateMethodOverloads(Matchers = [typeof(Class_24_Matcher)])]
public class Class_24_ClassTarget
{
    public void Case_1(int param_1, string? param_2) { }
}

[GenerateMethodOverloads(Matchers = [typeof(Class_24_Matcher)])]
public struct Class_24_StructTarget
{
    public void Case_1(int param_1, string? param_2) { }
}

[GenerateMethodOverloads(Matchers = [typeof(Class_24_Matcher)])]
public interface Class_24_InterfaceTarget
{
    void Case_1(int param_1, string? param_2);
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "PreferConcreteValueOverDefault")]
public static class Class_24_AcceptanceCriterias
{
    public static void Case_1(this Class_24_ClassTarget source, int param_1) =>
        source.Case_1(param_1, param_2: null);

    public static void Case_1(this Class_24_StructTarget source, int param_1) =>
        source.Case_1(param_1, param_2: null);

    public static void Case_1(this Class_24_InterfaceTarget source, int param_1) =>
        source.Case_1(param_1, param_2: null);
}

