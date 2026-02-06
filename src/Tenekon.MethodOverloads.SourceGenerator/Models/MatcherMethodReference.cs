namespace Tenekon.MethodOverloads.SourceGenerator.Models;

internal readonly record struct MatcherMethodReference(
    string ContainingTypeDisplay,
    string MethodName,
    int ParameterCount,
    string NamespaceName);