using System.Diagnostics.CodeAnalysis;
using Tenekon.MethodOverloads.SourceGenerator;

namespace Tenekon.MethodOverloads.AcceptanceCriterias;

public sealed class Class_30;

public static class Class_30_Target
{
    [GenerateOverloads(nameof(param_2))]
    public static void Case_1(this Class_30 source, int param_1, string? param_2) { }

    [SuppressMessage("MethodOverloadsGenerator", "MOG007")]
    [GenerateOverloads(nameof(param_2), Begin = nameof(param_1))]
    public static void Case_2(this Class_30 source, int param_1, string? param_2) { }
}

[SuppressMessage("ReSharper", "PreferConcreteValueOverDefault")]
public static class Class_30_AcceptanceCriterias
{
    public static void Case_1(this Class_30 source, int param_1) =>
        Class_30_Target.Case_1(source, param_1, param_2: null);
}
