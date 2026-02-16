#nullable enable
namespace Tenekon.MethodOverloads;

[global::Microsoft.CodeAnalysis.Embedded]
[global::System.AttributeUsage(global::System.AttributeTargets.Class, AllowMultiple = true)]
internal sealed class MatcherUsageAttribute : global::System.Attribute
{
    public MatcherUsageAttribute(string methodName)
    {
    }
}