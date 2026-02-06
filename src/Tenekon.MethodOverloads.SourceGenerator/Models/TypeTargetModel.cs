using Tenekon.MethodOverloads.SourceGenerator.Helpers;

namespace Tenekon.MethodOverloads.SourceGenerator.Models;

internal readonly record struct TypeTargetModel(
    TypeModel Type,
    bool HasGenerateMethodOverloads,
    EquatableArray<string> MatcherTypeDisplays);