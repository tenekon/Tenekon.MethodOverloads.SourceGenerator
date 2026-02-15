using System.Diagnostics.CodeAnalysis;

namespace Tenekon.MethodOverloads.AcceptanceCriterias.MatcherUsage;

/// <summary>
/// Matcher matches target, but overload generation is skipped due to defaults inside window.
/// </summary>
internal interface Class_MatcherUsage_1_Matcher
{
    [GenerateOverloads(nameof(optional))]
    void Match(int required, int optional);
}

[GenerateMethodOverloads(Matchers = [typeof(Class_MatcherUsage_1_Matcher)])]
public class Class_MatcherUsage_1
{
    [SuppressMessage("MethodOverloadsGenerator", "MOG003")]
    public void Case_1(int required, int optional = 42) { }
}