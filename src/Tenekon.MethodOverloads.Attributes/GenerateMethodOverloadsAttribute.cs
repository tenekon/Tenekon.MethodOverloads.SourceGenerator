#nullable enable
namespace Tenekon.MethodOverloads;

[global::System.AttributeUsage(
    global::System.AttributeTargets.Class
    | global::System.AttributeTargets.Struct
    | global::System.AttributeTargets.Interface,
    AllowMultiple = true)]
public sealed class GenerateMethodOverloadsAttribute : global::System.Attribute
{
    public global::System.Type[]? Matchers { get; set; }
}
