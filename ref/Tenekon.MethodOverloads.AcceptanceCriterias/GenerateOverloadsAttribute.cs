// ReSharper disable once CheckNamespace
namespace Tenekon.MethodOverloads.SourceGenerator;

[AttributeUsage(AttributeTargets.Method)]
public class GenerateOverloadsAttribute : Attribute
{
    /// <summary>
    /// All parameters beginning from <see cref="Begin"/> (inclusive) are considered for optional or required.
    /// </summary>
    public string? Begin { get; set; }

    /// <summary>
    /// All parameters after <see cref="BeginExclusive"/> are considered for optional or required.
    /// </summary>
    public string? BeginExclusive { get; set; }

    /// <summary>
    /// All parameters before <see cref="EndExclusive"/> are considered for optional or required.
    /// </summary>
    public string? EndExclusive { get; set; }

    /// <summary>
    /// All parameters until <see cref="Begin"/> (inclusive) are considered for optional or required.
    /// </summary>
    public string? End { get; set; }

    public Type[]? Matchers { get; set; }
}

