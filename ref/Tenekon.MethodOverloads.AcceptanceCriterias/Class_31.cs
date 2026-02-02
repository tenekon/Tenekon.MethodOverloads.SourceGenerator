using System.Diagnostics.CodeAnalysis;
using Tenekon.MethodOverloads.SourceGenerator;

namespace Tenekon.MethodOverloads.AcceptanceCriterias;

public abstract class Class_31
{
    [SuppressMessage("MethodOverloadsGenerator", "MOG008")]
    [GenerateOverloads(Begin = nameof(param_2), End = nameof(param_2))]
    public abstract void Case_1(int param_1, string? param_2);
}

[SuppressMessage("ReSharper", "PreferConcreteValueOverDefault")]
public static class Class_31_AcceptanceCriterias
{
    public static void Case_1(this Class_31 source, int param_1) =>
        source.Case_1(param_1, param_2: null);
}

