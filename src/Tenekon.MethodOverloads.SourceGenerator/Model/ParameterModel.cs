using Microsoft.CodeAnalysis;

namespace Tenekon.MethodOverloads.SourceGenerator.Model;

internal readonly record struct ParameterModel(
    string Name,
    string TypeDisplay,
    string SignatureTypeDisplay,
    RefKind RefKind,
    bool IsParams,
    bool IsOptional,
    bool HasExplicitDefaultValue,
    bool HasDefaultFromSyntax);

internal readonly record struct ParameterSignatureModel(string SignatureTypeDisplay, RefKind RefKind, bool IsParams);