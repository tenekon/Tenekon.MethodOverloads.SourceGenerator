using Tenekon.MethodOverloads.SourceGenerator.Helpers;
using Tenekon.MethodOverloads.SourceGenerator.Models;

namespace Tenekon.MethodOverloads.SourceGenerator.Parsing.Inputs;

internal readonly record struct MethodTargetInput(
    MethodModel Method,
    TypeModel ContainingType,
    EquatableArray<GenerateOverloadsAttributeModel> GenerateAttributesFromAttribute,
    EquatableArray<GenerateOverloadsAttributeModel> GenerateAttributesFromSyntax,
    EquatableArray<string> MatcherTypeDisplays,
    EquatableArray<MatcherTypeModel> MatcherTypes);