using Tenekon.MethodOverloads.SourceGenerator.Helpers;

namespace Tenekon.MethodOverloads.SourceGenerator.Model;

internal readonly record struct TypeTargetModel(
    TypeModel Type,
    bool HasGenerateMethodOverloads,
    EquatableArray<string> MatcherTypeDisplays,
    OverloadOptionsModel Options);

internal readonly record struct MethodTargetModel(
    MethodModel Method,
    bool HasGenerateOverloads,
    GenerateOverloadsArgsModel? GenerateArgsFromAttribute,
    GenerateOverloadsArgsModel? GenerateArgsFromSyntax,
    EquatableArray<string> MatcherTypeDisplays,
    OverloadOptionsModel OptionsFromAttributeOrSyntax,
    OverloadOptionsModel? SyntaxOptions,
    bool OptionsFromAttribute);

internal readonly record struct MatcherTypeModel(
    TypeModel Type,
    OverloadOptionsModel Options,
    EquatableArray<MatcherMethodModel> MatcherMethods);

internal readonly record struct MatcherMethodModel(
    MethodModel Method,
    GenerateOverloadsArgsModel? GenerateArgsFromAttribute,
    GenerateOverloadsArgsModel? GenerateArgsFromSyntax,
    OverloadOptionsModel OptionsFromAttributeOrSyntax,
    OverloadOptionsModel? SyntaxOptions);

internal readonly record struct MatcherMethodReference(
    string ContainingTypeDisplay,
    string MethodName,
    int ParameterCount,
    string NamespaceName);

internal readonly record struct GenerateOverloadsArgsModel(
    string? BeginEnd,
    string? Begin,
    string? BeginExclusive,
    string? End,
    string? EndExclusive,
    SourceLocationModel? AttributeLocation,
    SourceLocationModel? MethodIdentifierLocation,
    SourceLocationModel? SyntaxAttributeLocation)
{
    public bool HasAny =>
        !string.IsNullOrEmpty(BeginEnd) || !string.IsNullOrEmpty(Begin) || !string.IsNullOrEmpty(BeginExclusive)
        || !string.IsNullOrEmpty(End) || !string.IsNullOrEmpty(EndExclusive);
}