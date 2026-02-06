using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Tenekon.MethodOverloads.SourceGenerator.Helpers;

internal static class RoslynHelpers
{
    public static readonly SymbolDisplayFormat TypeDisplayFormat =
        SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(
            SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
            | SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

    public static readonly SymbolDisplayFormat SignatureDisplayFormat = SymbolDisplayFormat.FullyQualifiedFormat;

    public static AttributeData? GetAttribute(ISymbol symbol, string attributeName)
    {
        foreach (var attribute in symbol.GetAttributes())
        {
            var name = attribute.AttributeClass?.Name;
            if (name is null) continue;

            if (string.Equals(name, attributeName, StringComparison.Ordinal)
                || (attributeName.EndsWith("Attribute", StringComparison.Ordinal) && string.Equals(
                    name,
                    attributeName.Substring(startIndex: 0, attributeName.Length - "Attribute".Length),
                    StringComparison.Ordinal)))
                return attribute;
        }

        return null;
    }

    public static ImmutableArray<AttributeData> GetAttributes(ISymbol symbol, string attributeName)
    {
        var builder = ImmutableArray.CreateBuilder<AttributeData>();

        foreach (var attribute in symbol.GetAttributes())
        {
            var name = attribute.AttributeClass?.Name;
            if (name is null) continue;

            if (string.Equals(name, attributeName, StringComparison.Ordinal)
                || (attributeName.EndsWith("Attribute", StringComparison.Ordinal) && string.Equals(
                    name,
                    attributeName.Substring(startIndex: 0, attributeName.Length - "Attribute".Length),
                    StringComparison.Ordinal)))
                builder.Add(attribute);
        }

        return builder.ToImmutable();
    }

    public static bool IsAttributeNameMatch(string name, string expected)
    {
        if (string.Equals(name, expected, StringComparison.Ordinal) || string.Equals(
                name,
                expected + "Attribute",
                StringComparison.Ordinal))
            return true;

        return name.EndsWith("." + expected, StringComparison.Ordinal) || name.EndsWith(
            "." + expected + "Attribute",
            StringComparison.Ordinal);
    }
}