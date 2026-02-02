// ReSharper disable once CheckNamespace

namespace Tenekon.MethodOverloads.SourceGenerator;

[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct)]
public class GenerateMethodOverloadsAttribute : Attribute
{
    public Type[]? Matchers { get; set; }
}