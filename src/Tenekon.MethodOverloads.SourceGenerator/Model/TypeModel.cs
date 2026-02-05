using Tenekon.MethodOverloads.SourceGenerator.Helpers;

namespace Tenekon.MethodOverloads.SourceGenerator.Model;

internal readonly record struct TypeModel(
    string DisplayName,
    string NamespaceName,
    EquatableArray<MethodModel> Methods,
    EquatableArray<MethodSignatureModel> MethodSignatures,
    OverloadOptionsModel Options);