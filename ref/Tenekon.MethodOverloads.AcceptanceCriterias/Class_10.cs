using System.Diagnostics.CodeAnalysis;

namespace Tenekon.MethodOverloads.AcceptanceCriterias;

/// <summary>
/// Existing optional/default parameters inside the optional span.
/// Expects no generated overloads that would be redundant.
/// </summary>
public abstract class Class_10
{
    [SuppressMessage("MethodOverloadsGenerator", "MOG003")]
    [GenerateOverloads(Begin = nameof(param_2), End = nameof(param_3))]
    public abstract void Case_1(int param_1, string? param_2 = null, bool param_3 = false);
}

/// <summary>
/// Expected extension overloads (none) for Class_10.
/// </summary>
public static class Class_10_AcceptanceCriterias
{
    // No extension methods for Case_1 (all omitted parameters already have defaults).
}