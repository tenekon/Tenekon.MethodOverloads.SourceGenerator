// ReSharper disable once CheckNamespace
namespace Tenekon.MethodOverloads.SourceGenerator;

[AttributeUsage(AttributeTargets.Class)]
public class GenerateMethodOverloadsAttribute : Attribute
{
    public Type[]? Matchers { get; set; }
}

