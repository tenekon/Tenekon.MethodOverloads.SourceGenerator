using Tenekon.MethodOverloads.SourceGenerator.Helpers;

namespace Tenekon.MethodOverloads.SourceGenerator.Models;

internal readonly record struct MethodTargetModel(
    MethodModel Method,
    bool HasGenerateOverloads,
    EquatableArray<GenerateOverloadsAttributeModel> GenerateAttributesFromAttribute,
    EquatableArray<GenerateOverloadsAttributeModel> GenerateAttributesFromSyntax,
    EquatableArray<string> MatcherTypeDisplays);