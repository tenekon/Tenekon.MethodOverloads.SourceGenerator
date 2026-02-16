using Tenekon.MethodOverloads.SourceGenerator.Helpers;

namespace Tenekon.MethodOverloads.SourceGenerator.Models;

internal readonly record struct GenerateOverloadsArgsModel(
    string? BeginEnd,
    string? Begin,
    string? BeginExclusive,
    string? End,
    string? EndExclusive,
    EquatableArray<string> ExcludeAny,
    bool HasInvalidExcludeAny,
    SourceLocationModel? InvalidExcludeAnyLocation,
    SourceLocationModel? AttributeLocation,
    SourceLocationModel? MethodIdentifierLocation,
    SourceLocationModel? SyntaxAttributeLocation)
{
    public bool HasAny =>
        !string.IsNullOrEmpty(BeginEnd) || !string.IsNullOrEmpty(Begin) || !string.IsNullOrEmpty(BeginExclusive)
        || !string.IsNullOrEmpty(End) || !string.IsNullOrEmpty(EndExclusive);

    public bool HasExcludeAny => HasInvalidExcludeAny || ExcludeAny.Items.Length > 0;
}
