using Microsoft.CodeAnalysis;

namespace Tenekon.MethodOverloads.SourceGenerator.Models;

internal readonly record struct BucketTypeModel(
    string Name,
    string Namespace,
    string DisplayName,
    Accessibility Accessibility,
    bool IsValid,
    string? InvalidReason,
    SourceLocationModel? AttributeLocation);
