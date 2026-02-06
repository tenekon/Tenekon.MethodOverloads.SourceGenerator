using Tenekon.MethodOverloads.SourceGenerator.Helpers;

namespace Tenekon.MethodOverloads.SourceGenerator.Models;

internal readonly record struct MatcherMethodModel(
    MethodModel Method,
    EquatableArray<GenerateOverloadsAttributeModel> GenerateAttributesFromAttribute,
    EquatableArray<GenerateOverloadsAttributeModel> GenerateAttributesFromSyntax);