#nullable enable
namespace Tenekon.MethodOverloads;

[global::System.AttributeUsage(global::System.AttributeTargets.Method, AllowMultiple = true)]
public sealed class GenerateOverloadsAttribute : global::System.Attribute
{
    public GenerateOverloadsAttribute()
    {
    }

    public GenerateOverloadsAttribute(string beginEnd)
    {
        Begin = beginEnd;
        End = beginEnd;
    }

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
    /// All parameters until <see cref="End"/> (inclusive) are considered for optional or required.
    /// </summary>
    public string? End { get; set; }

    public global::System.Type[]? Matchers { get; set; }
}