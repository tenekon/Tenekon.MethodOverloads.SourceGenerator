namespace Tenekon.MethodOverloads.SourceGenerator.Models;

internal readonly record struct SupplyParameterTypeModel(
    int ScopeId,
    string TypeParameterName,
    string SuppliedTypeDisplay,
    string SuppliedSignatureTypeDisplay,
    bool IsValid,
    string? InvalidReason,
    SourceLocationModel? AttributeLocation,
    SourceLocationModel? NameLocation,
    SourceLocationModel? TypeLocation);
