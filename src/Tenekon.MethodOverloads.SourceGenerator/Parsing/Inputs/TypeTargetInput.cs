using Tenekon.MethodOverloads.SourceGenerator.Helpers;
using Tenekon.MethodOverloads.SourceGenerator.Models;

namespace Tenekon.MethodOverloads.SourceGenerator.Parsing.Inputs;

internal readonly record struct TypeTargetInput(
    TypeModel Type,
    EquatableArray<string> MatcherTypeDisplays,
    EquatableArray<MatcherTypeModel> MatcherTypes);