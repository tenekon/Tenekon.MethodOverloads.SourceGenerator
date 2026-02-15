using Microsoft.CodeAnalysis;

namespace Tenekon.MethodOverloads.SourceGenerator.Models;

internal readonly record struct MatcherMethodReference(
    string ContainingTypeDisplay,
    Accessibility ContainingTypeAccessibility,
    string MethodName,
    int ParameterCount,
    string NamespaceName);
