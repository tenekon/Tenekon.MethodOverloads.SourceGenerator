#nullable enable
namespace Tenekon.MethodOverloads;

/// <summary>
/// Marks a type as a matcher. Its <see cref="GenerateOverloadsAttribute"/> methods describe windows for the targets that
/// list the type in <c>Matchers</c>. A matcher type is never a generation target itself.
/// </summary>
[global::Microsoft.CodeAnalysis.Embedded]
[global::System.AttributeUsage(
    global::System.AttributeTargets.Class
    | global::System.AttributeTargets.Struct
    | global::System.AttributeTargets.Interface)]
internal sealed class OverloadMatcherAttribute : global::System.Attribute
{
}
