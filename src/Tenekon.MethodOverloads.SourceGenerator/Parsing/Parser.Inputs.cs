using Tenekon.MethodOverloads.SourceGenerator.Helpers;
using Tenekon.MethodOverloads.SourceGenerator.Model;

namespace Tenekon.MethodOverloads.SourceGenerator.Parsing;

internal readonly record struct TypeTargetInput(
    TypeModel Type,
    EquatableArray<string> MatcherTypeDisplays,
    EquatableArray<MatcherTypeModel> MatcherTypes,
    SourceFileModel SourceFile);

internal readonly record struct MethodTargetInput(
    MethodModel Method,
    TypeModel ContainingType,
    GenerateOverloadsArgsModel? GenerateArgsFromAttribute,
    GenerateOverloadsArgsModel? GenerateArgsFromSyntax,
    EquatableArray<string> MatcherTypeDisplays,
    OverloadOptionsModel OptionsFromAttributeOrSyntax,
    OverloadOptionsModel? SyntaxOptions,
    bool OptionsFromAttribute,
    EquatableArray<MatcherTypeModel> MatcherTypes,
    SourceFileModel SourceFile);
