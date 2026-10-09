using System.Diagnostics.CodeAnalysis;

namespace Tenekon.MethodOverloads.AcceptanceCriterias;

/// <summary>
/// Not marked with [OverloadMatcher], so it is no matcher and keeps its own role:
/// its GenerateOverloads method is a direct target, even though Class_46 lists the type in Matchers.
/// </summary>
public interface Class_46_Service
{
    [GenerateOverloads(nameof(param_b))]
    void Case_1(int param_a, string? param_b);
}

/// <summary>
/// An unmarked type in Matchers is ignored and produces MOG020 at every typeof reference: here once on the type and
/// once on Case_2.
/// </summary>
[SuppressMessage("MethodOverloadsGenerator", "MOG020")]
[GenerateMethodOverloads(Matchers = [typeof(Class_46_Service)])]
public sealed class Class_46
{
    public void Case_1(int param_1, string? param_2) { }

    [GenerateOverloads(Matchers = [typeof(Class_46_Service)])]
    public void Case_2(int param_1, string? param_2) { }
}

/// <summary>
/// Overloads are expected for Class_46_Service only; Class_46 gets none.
/// </summary>
public static class Class_46_AcceptanceCriterias
{
    public static void Case_1(this Class_46_Service source, int param_a)
    {
        source.Case_1(param_a, param_b: null);
    }
}
