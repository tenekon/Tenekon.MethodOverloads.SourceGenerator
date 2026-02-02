using System.Diagnostics.CodeAnalysis;
using Tenekon.MethodOverloads.SourceGenerator;

namespace Tenekon.MethodOverloads.AcceptanceCriterias;

public abstract class Class_32
{
    [SuppressMessage("MethodOverloadsGenerator", "MOG009")]
    [GenerateOverloads(Begin = nameof(param_1), BeginExclusive = nameof(param_1))]
    public abstract void Case_1(int param_1, string? param_2);

    [SuppressMessage("MethodOverloadsGenerator", "MOG010")]
    [GenerateOverloads(End = nameof(param_2), EndExclusive = nameof(param_2))]
    public abstract void Case_2(int param_1, string? param_2);
}

public static class Class_32_AcceptanceCriterias
{
    // No extension methods expected for Case_1 and Case_2 due to conflicting anchors.
}
