using Microsoft.CodeAnalysis;
using Tenekon.MethodOverloads.SourceGenerator.Helpers;

namespace Tenekon.MethodOverloads.SourceGenerator.Models;

internal readonly record struct TypeModel(
    string DisplayName,
    string NamespaceName,
    Accessibility DeclaredAccessibility,
    EquatableArray<MethodModel> Methods,
    EquatableArray<MethodSignatureModel> MethodSignatures,
    OverloadOptionsModel Options);
