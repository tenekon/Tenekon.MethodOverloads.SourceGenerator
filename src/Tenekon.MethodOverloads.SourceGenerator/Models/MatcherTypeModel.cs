using Tenekon.MethodOverloads.SourceGenerator.Helpers;

namespace Tenekon.MethodOverloads.SourceGenerator.Models;

internal readonly record struct MatcherTypeModel(
    TypeModel Type,
    OverloadOptionsModel Options,
    EquatableArray<MatcherMethodModel> MatcherMethods);