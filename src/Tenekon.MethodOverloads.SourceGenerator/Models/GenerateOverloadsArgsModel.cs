namespace Tenekon.MethodOverloads.SourceGenerator.Models;

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