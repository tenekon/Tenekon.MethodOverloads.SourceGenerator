using Microsoft.CodeAnalysis;
using Tenekon.MethodOverloads.SourceGenerator.Helpers;

namespace Tenekon.MethodOverloads.SourceGenerator.Model;

internal readonly record struct MethodSignatureModel(
    string Name,
    int TypeParameterCount,
    EquatableArray<ParameterSignatureModel> Parameters);

internal readonly record struct MethodModel(
    string Name,
    string ContainingTypeDisplay,
    string ContainingNamespace,
    string ReturnTypeDisplay,
    bool IsStatic,
    bool IsExtensionMethod,
    Accessibility DeclaredAccessibility,
    int TypeParameterCount,
    EquatableArray<string> TypeParameterNames,
    string TypeParameterConstraints,
    EquatableArray<ParameterModel> Parameters,
    SourceLocationModel? IdentifierLocation,
    bool IsOrdinary,
    OverloadOptionsModel Options,
    bool OptionsFromAttribute);
